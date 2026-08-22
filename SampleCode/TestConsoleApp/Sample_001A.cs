using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.Core.Attributes;
using Sentinel.Diagnostics.Core.Runtime.Context;
using System.Diagnostics;

namespace SampleCode.TestConsoleApp;
public sealed class DemoServiceA
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
            // Missing Add: logger.LogException(ex);
            //logger.LogException(ex);
            throw;
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
            if (b > 0)
            {
                logger.Info($"Division Test! {a} / {b}");
            }
            else
            {
                logger.Info($"Division by Zero Test! {a} / {b}");
            }

            return Mathematics.Divide(a, b);
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
            throw;
        }
        finally
        {
            Debug.WriteLine("Test!");
        // Missing Add: pop statment
        }
    }
}

    [AutoLog]
    public static int Divide2(int a, int b)
{
    var parent = SentinelOperationContext.CurrentOperationId;
    var op = Guid.NewGuid();
    SentinelOperationContext.CurrentOperationId = op;
    using (var logger = new AutoLogger(new AutoLogMetadata("Divide2", "SampleCode.TestConsoleApp.DemoService.Divide2", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.DemoService.Divide2"), parent, op))
    {
        try
        {
            if (b > 0)
            {
                Console.WriteLine($"Division Test! {a} / {b}");
            }
            else
            {
                Console.WriteLine($"Division by Zero Test! {a} / {b}");
            }

            return Mathematics.Divide(a, b);
        }
        catch (Exception ex)
        {
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