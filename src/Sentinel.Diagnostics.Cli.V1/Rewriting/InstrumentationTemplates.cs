namespace Sentinel.Diagnostics.Cli.Rewriting;

public static class InstrumentationTemplates
{
    // ------------------------------------------------------------
    // ENTER / EXIT LOGGING
    // ------------------------------------------------------------
    public static string LogEnter(string methodName)
        => $"SentinelLogger.Enter(\"{methodName}\");";

    public static string LogExit(string methodName)
        => $"SentinelLogger.Exit(\"{methodName}\");";

    // ------------------------------------------------------------
    // EXCEPTION LOGGING
    // ------------------------------------------------------------
    public static string LogException(string methodName)
        => $"SentinelLogger.Exception(\"{methodName}\", ex);";

    // ------------------------------------------------------------
    // DURATION MEASUREMENT
    // ------------------------------------------------------------
    public static string DurationStart()
        => "var __sentinelStart = System.Diagnostics.Stopwatch.StartNew();";

    public static string DurationEnd(string methodName)
        => $"SentinelLogger.Duration(\"{methodName}\", __sentinelStart.ElapsedMilliseconds);";

    // ------------------------------------------------------------
    // PARAMETER LOGGING
    // ------------------------------------------------------------
    public static string LogParameter(string paramName)
        => $"SentinelLogger.Parameter(\"{paramName}\", {paramName});";

    public static IEnumerable<string> LogParameters(IEnumerable<string> paramNames)
    {
        foreach (var p in paramNames)
            yield return LogParameter(p);
    }

    // ------------------------------------------------------------
    // RETRY LOGIC (statement-by-statement, safe)
    // ------------------------------------------------------------
    public static IEnumerable<string> RetryStatements(
        string methodName,
        int retryCount,
        int retryDelayMs)
    {
        yield return $"int __sentinelRetries = {retryCount};";
        yield return "while (true)";
        yield return "{";
        yield return "    try";
        yield return "    {";
        yield return "        // __SENTINEL_ORIGINAL_BODY__";
        yield return "        break;";
        yield return "    }";
        yield return "    catch (Exception ex)";
        yield return "    {";
        yield return $"        if (__sentinelRetries-- <= 0)";
        yield return "        {";
        yield return $"            SentinelLogger.Exception(\"{methodName}\", ex);";
        yield return "            throw;";
        yield return "        }";
        yield return "";
        yield return $"        SentinelLogger.Retry(\"{methodName}\", __sentinelRetries);";
        yield return $"        System.Threading.Thread.Sleep({retryDelayMs});";
        yield return "    }";
        yield return "}";
    }
}
