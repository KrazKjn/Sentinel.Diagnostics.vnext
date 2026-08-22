using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.Core.Attributes;
using Sentinel.Diagnostics.Core.Runtime.Context;

namespace SampleCode.TestConsoleApp
{
    internal class Mathematics
    {
        [AutoLog]
        public static int Add(int a, int b)
        {
            var parent = SentinelOperationContext.CurrentOperationId;
            var op = Guid.NewGuid();
            SentinelOperationContext.CurrentOperationId = op;

            using (var logger = new AutoLogger(new AutoLogMetadata("Add", "SampleCode.TestConsoleApp.Mathematics.Add", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.Mathematics.Add"), parent, op))
            {
                try
                {
                    logger.Info($"Add Test! {a} + {b}");
                    logger.Debug("Dubug Message Level 1+", 1);
                    logger.Debug("Dubug Message Level 2+", 2);
                    logger.Debug("Dubug Message Level 3+", 3);
                    logger.Debug("Dubug Message Level 4+", 4);
                    logger.Warn($"Add Warn Test! {a} + {b}");
                    return a + b;
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

        [AutoLog]
        public static int Divide(int a, int b)
        {
            var parent = SentinelOperationContext.CurrentOperationId;
            var op = Guid.NewGuid();
            SentinelOperationContext.CurrentOperationId = op;

            using (var logger = new AutoLogger(new AutoLogMetadata("Divide", "SampleCode.TestConsoleApp.Mathematics.Divide", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.Mathematics.Divide"), parent, op))
            {
                try
                {
                    logger.Info($"Division Test! {a} / {b}");
                    logger.Debug("Dubug Message Level 1+", 1);
                    logger.Debug("Dubug Message Level 2+", 2);
                    logger.Debug("Dubug Message Level 3+", 3);
                    logger.Debug("Dubug Message Level 4+", 4);
                    if (b <= 0)
                    {
                        logger.Warn($"Division by Zero Test! {a} / {b}");
                    }
                    return a / b;
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
}
