using Sentinel.Diagnostics.AutoLogRuntime.Context;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;
using Sentinel.Diagnostics.Core.Attributes;
using TestConsoleApp;

namespace SampleCode.TestConsoleApp;
public sealed class DemoService
{
    [AutoLog]
    public static int Divide(int a, int b)
    {
        using (var logger = new AutoLogger(new AutoLogMetadata("Divide", "SampleCode.TestConsoleApp.DemoService.Divide", new AutoLogParameter[] { new AutoLogParameter("a", typeof(int), a), new AutoLogParameter("b", typeof(int), b) }, AutoLoggerContext.CurrentDepth, Guid.NewGuid(), "SampleCode.TestConsoleApp.DemoService.Divide")))
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
        }
    }
}