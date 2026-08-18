using SampleCode.TestConsoleApp;
using Sentinel.Diagnostics.AutoLogRuntime.Diagnostics;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AutoLoggerConfig.LoadFromFile("autologger.json");

            Console.WriteLine("Hello, World!");
            TestDivide(1, 0);
        }

        static int TestDivide(int a, int b)
        {
            return DemoService.Divide(a, b);
        }
    }
}
