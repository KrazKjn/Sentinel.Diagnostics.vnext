using Microsoft.CodeAnalysis;
using Sentinel.Diagnostics.Generator.Diagnostics;
using Sentinel.Diagnostics.Generator.Metadata;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Sentinel.Diagnostics.Generator.Validation;

public static class MetadataValidator
{
    // ---------------------------------------------------------------------
    // PUBLIC API — Production pipeline
    // ---------------------------------------------------------------------
    public static ValidatedMethodMetadata? Validate(
        RawMethodMetadata raw,
        SourceProductionContext context)
    {
        return ValidateInternal(
            raw,
            diagnostic => context.ReportDiagnostic(diagnostic));
    }

    //// ---------------------------------------------------------------------
    //// PUBLIC API — Test pipeline (FakeSourceProductionContext)
    //// ---------------------------------------------------------------------
    //public static ValidatedMethodMetadata? Validate(
    //    RawMethodMetadata raw,
    //    FakeSourceProductionContext context)
    //{
    //    return ValidateInternal(
    //        raw,
    //        diagnostic => context.ReportDiagnostic(diagnostic));
    //}

    // ---------------------------------------------------------------------
    // INTERNAL VALIDATION ENGINE — Shared by both overloads
    // ---------------------------------------------------------------------
    public static ValidatedMethodMetadata? ValidateInternal(
        RawMethodMetadata raw,
        Action<Diagnostic> reportDiagnostic)
    {
        if (raw is null)
            return null;

        // ---------------------------------------------------------------------
        // RULE 1 — AutoLog attribute correctness
        // ---------------------------------------------------------------------
        if (string.IsNullOrWhiteSpace(raw.Options.Policy))
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidPolicyName,
                    raw.MethodLocation,
                    raw.Options.Policy ?? "<null>",
                    "Policy cannot be null, empty, or whitespace"));

            return null;
        }

        if (string.IsNullOrWhiteSpace(raw.Options.Span))
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidSpanName,
                    raw.MethodLocation,
                    raw.Options.Span ?? "<null>",
                    "Span cannot be null, empty, or whitespace"));

            return null;
        }

        // ---------------------------------------------------------------------
        // RULE 2 — Unsupported method kinds
        // ---------------------------------------------------------------------
        if (raw.IsAsync && raw.IsIterator)
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedMethodKind,
                    raw.MethodLocation,
                    raw.MethodName));

            return null;
        }

        if (raw.IsIterator)
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.UnsupportedMethodKind,
                    raw.MethodLocation,
                    raw.MethodName));

            return null;
        }

        // ---------------------------------------------------------------------
        // RULE 3 — Sensitive parameter validation
        // ---------------------------------------------------------------------
        foreach (var p in raw.Parameters)
        {
            if (!p.IsSensitive)
                continue;

            if (p.RefKind is RefKind.Ref or RefKind.Out or RefKind.In)
            {
                reportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidSensitiveParameter,
                        raw.MethodLocation,
                        p.Name));

                return null;
            }

            if (p.FullyQualifiedTypeName is not null &&
                IsStructType(p.FullyQualifiedTypeName))
            {
                reportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidSensitiveParameter,
                        raw.MethodLocation,
                        p.Name));

                return null;
            }
        }

        // ---------------------------------------------------------------------
        // RULE 4 — CancellationToken usage rules
        // ---------------------------------------------------------------------
        int ctCount = raw.Parameters.Count(p =>
            p.FullyQualifiedTypeName == "global::System.Threading.CancellationToken");

        if (ctCount > 1)
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidCancellationTokenUsage,
                    raw.MethodLocation,
                    raw.MethodName,
                    "More than one CancellationToken parameter is not allowed"));

            return null;
        }

        if (ctCount == 1)
        {
            var last = raw.Parameters.Last();
            if (last.FullyQualifiedTypeName != "global::System.Threading.CancellationToken")
            {
                reportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticDescriptors.InvalidCancellationTokenUsage,
                        raw.MethodLocation,
                        raw.MethodName,
                        "CancellationToken must be the last parameter"));

                return null;
            }
        }

        // ---------------------------------------------------------------------
        // RULE 5 — Span validation
        // ---------------------------------------------------------------------
        string validatedSpan = raw.Options.Span!.Trim();

        if (validatedSpan.Length == 0)
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidSpanName,
                    raw.MethodLocation,
                    validatedSpan,
                    "Span cannot be empty after trimming"));

            return null;
        }

        if (validatedSpan.Contains(' '))
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidSpanName,
                    raw.MethodLocation,
                    validatedSpan,
                    "Span cannot contain whitespace"));

            return null;
        }

        // ---------------------------------------------------------------------
        // RULE 6 — Policy validation
        // ---------------------------------------------------------------------
        string validatedPolicy = raw.Options.Policy!.Trim();

        if (validatedPolicy.Length == 0)
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidPolicyName,
                    raw.MethodLocation,
                    validatedPolicy,
                    "Policy cannot be empty after trimming"));

            return null;
        }

        if (validatedPolicy.Contains(' '))
        {
            reportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.InvalidPolicyName,
                    raw.MethodLocation,
                    validatedPolicy,
                    "Policy cannot contain whitespace"));

            return null;
        }

        // ---------------------------------------------------------------------
        // RULE 7 — Build validated parameter metadata
        // ---------------------------------------------------------------------
        var validatedParameters =
            raw.Parameters.Select(p =>
                new ValidatedParameterMetadata(
                    Name: p.Name,
                    FullyQualifiedTypeName: p.FullyQualifiedTypeName!,
                    RefKind: p.RefKind,
                    IsSensitive: p.IsSensitive,
                    IsParams: p.IsParams,
                    IsNullable: p.IsNullable,
                    HasExplicitDefaultValue: p.HasExplicitDefaultValue,
                    DefaultValueExpression: p.DefaultValueExpression,
                    ShouldLog: p.ShouldLog))
            .ToImmutableArray();

        // ---------------------------------------------------------------------
        // RULE 8 — Construct validated metadata
        // ---------------------------------------------------------------------
        return new ValidatedMethodMetadata(
            MethodName: raw.MethodName,
            MethodLocation: raw.MethodLocation,
            FullyQualifiedMethodName: raw.FullyQualifiedMethodName,
            DeclaringNamespace: raw.DeclaringNamespace,
            DeclaringTypeName: raw.DeclaringTypeName,
            FullyQualifiedDeclaringTypeName: raw.FullyQualifiedDeclaringTypeName,
            ContainingTypes: raw.ContainingTypes,
            ReturnTypeName: raw.ReturnTypeName,
            FullyQualifiedReturnTypeName: raw.FullyQualifiedReturnTypeName,
            Options: raw.Options,
            IsAsync: raw.IsAsync,
            IsIterator: raw.IsIterator,
            IsStatic: raw.IsStatic,
            HasCancellationToken: raw.HasCancellationToken,
            GenericTypeParameters: raw.GenericTypeParameters,
            MethodAccessibility: raw.MethodAccessibility,
            DeclaringTypeAccessibility: raw.DeclaringTypeAccessibility,
            Parameters: validatedParameters);
    }

    // -------------------------------------------------------------------------
    // Helper: detect struct types (simple heuristic)
    // -------------------------------------------------------------------------
    private static bool IsStructType(string fullyQualifiedTypeName)
    {
        return fullyQualifiedTypeName.StartsWith("global::System.ValueTuple")
            || fullyQualifiedTypeName.Contains("struct");
    }
}
