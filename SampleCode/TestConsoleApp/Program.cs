using SampleCode.TestConsoleApp;

namespace TestConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            TestDivide(1, 0);
        }

        static int TestDivide(int a, int b)
        {
            return DemoService.Divide(a, b);
        }
    }
}
