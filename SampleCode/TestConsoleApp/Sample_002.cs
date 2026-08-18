using Sentinel.Diagnostics.Core.Attributes;

namespace SampleCode.TestConsoleApp
{
    public sealed class DemoService2
    {
        [AutoLog]
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}