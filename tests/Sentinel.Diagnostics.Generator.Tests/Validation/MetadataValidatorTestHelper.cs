using Sentinel.Diagnostics.Generator.Metadata;
using Sentinel.Diagnostics.Generator.Validation;

namespace Sentinel.Diagnostics.Generator.Tests.Validation;

internal static class MetadataValidatorTestHelper
{
    public static ValidatedMethodMetadata? ValidateForTest(
        RawMethodMetadata raw,
        FakeSourceProductionContext context)
    {
        return MetadataValidator.ValidateInternal(
            raw,
            diagnostic => context.ReportDiagnostic(diagnostic));
    }
}