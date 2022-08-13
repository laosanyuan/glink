#define OnlyTestMA5

using BenchmarkDotNet.Running;
#if !OnlyTestMA5
using BenchmarkDotNet.Attributes;
using System.Reflection;
#endif

namespace Glink.Components.BenchmarkTests
{
    class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            var ma5 = new TestMA5();
            ma5.DataCount = 100000;
            ma5.Setup();
            ma5.TestExecute();
#else
#if OnlyTestMA5
            var summary = BenchmarkRunner.Run<TestMA5>();
#else
            // 运行全部Benmark测试
            var benchmarks = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Any(m => m.GetCustomAttributes(typeof(BenchmarkAttribute), false).Any()))
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name)
                .ToArray();
            var benchmarkSwitcher = new BenchmarkSwitcher(benchmarks);
            benchmarkSwitcher.RunAll();
#endif
            Console.ReadLine();
#endif
        }
    }
}
