using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.Core.Attributes;

namespace SampleCode.TestConsoleApp
{
    internal class Mathematics
    {
        [AutoLog]
        public static int Add(int a, int b)
        {
            using (var logger = new AutoLogger(new AutoLogMetadata("Add", "SampleCode.TestConsoleApp.Mathematics.Add", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.Mathematics.Add")))
            {
                try
                {
                    logger.Info($"Add Test! {a} + {b}");
                    logger.Debug("Dubug Message Level 1+", 1);
                    logger.Debug("Dubug Message Level 2+", 2);
                    logger.Debug("Dubug Message Level 3+", 3);
                    logger.Debug("Dubug Message Level 4+", 4);
                    logger.Warning($"Add Warn Test! {a} + {b}");
                    return a + b;
                }
                catch (Exception ex)
                {
                    logger.LogException(ex);
                    throw;
                }
            }
        }

        [AutoLog]
        public static int Divide(int a, int b)
        {
            using (var logger = new AutoLogger(new AutoLogMetadata("Divide", "SampleCode.TestConsoleApp.Mathematics.Divide", "Method", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.Mathematics.Divide")))
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
                        logger.Warning($"Division by Zero Test! {a} / {b}");
                    }
                    return a / b;
                }
                catch (Exception ex)
                {
                    logger.LogException(ex);
                    throw;
                }
            }
        }
    }
}
