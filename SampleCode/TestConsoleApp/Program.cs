using SampleCode.TestConsoleApp;
using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.AutoLogRuntime.Logging;
using Sentinel.Diagnostics.Core.Runtime.Context;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // First we need to load the Logger Configuration
            AutoLoggerConfig.LoadFromFile("autologger.json");
            
            var parent = SentinelOperationContext.CurrentOperationId;
            var op = Guid.NewGuid();
            SentinelOperationContext.CurrentOperationId = op;

            using (var logger = new AutoLogger(new AutoLogMetadata("Main", "SampleCode.Program.Main", "Method", Array.Empty<AutoLogParameter>(), AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.Program.Main"), parent, op))
            {
                try
                {
                    logger.Info("Hello, World!");
                    logger.Info($"Adding 1 + 0 = {TestAdd(1, 0)}");
                    logger.Info($"Dividing 1 / 0 = {TestDivide(1, 0)}");
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

        static int TestAdd(int a, int b)
        {
            return DemoService.Add(a, b);
        }

        static int TestDivide(int a, int b)
        {
            return DemoServiceA.Divide(a, b);
        }
    }
}
