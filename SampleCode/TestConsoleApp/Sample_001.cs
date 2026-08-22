using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.Core.Attributes;
using Sentinel.Diagnostics.Core.Runtime.Context;
using System.Diagnostics;

namespace SampleCode.TestConsoleApp;
public sealed class DemoService
{
    [AutoLog]
    public static int Add(int a, int b)
{
    var parent = SentinelOperationContext.CurrentOperationId;
    var op = Guid.NewGuid();
    SentinelOperationContext.CurrentOperationId = op;
    using (var logger = new AutoLogger(new AutoLogMetadata("Add", "SampleCode.TestConsoleApp.DemoService.Add", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.DemoService.Add"), parent, op))
    {
        try
        {
            logger.Info($"Add Test! {a} + {b}");
            return Mathematics.Add(a, b);
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
            // Missing Add: logger.LogException(ex);
            //logger.LogException(ex);
            throw;
        }
        finally
        {
            SentinelOperationContext.CurrentOperationId = parent;
        }
    // Missing Add: finally
    }
}

    [AutoLog]
    public static int Divide(int a, int b)
{
    var parent = SentinelOperationContext.CurrentOperationId;
    var op = Guid.NewGuid();
    SentinelOperationContext.CurrentOperationId = op;
    using (var logger = new AutoLogger(new AutoLogMetadata("Divide", "SampleCode.TestConsoleApp.DemoService.Divide", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.DemoService.Divide"), parent, op))
    {
        try
        {
            logger.Info($"Divide Test! {a} + {b}");
            logger.Info($"INFO: Divide Test! {a} + {b}");
            logger.Info($"INFORMATION: Divide Test! {a} + {b}");
            logger.Warn($"WARN: Divide Test! {a} + {b}");
            logger.Warn($"WARNING: Divide Test! {a} + {b}");
            return Mathematics.Divide(a, b);
        }
        catch (Exception ex)
        {
            logger.Error($"ERROR: Exception {ex}");
            logger.Error(ex);
            logger.LogException(ex);
            throw;
        }
        finally
        {
            SentinelOperationContext.CurrentOperationId = parent;
        }
    }
}
}