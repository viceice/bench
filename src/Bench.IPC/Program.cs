using BenchmarkDotNet.Running;
using System.Reflection;

namespace Bench.IPC
{
    class Program
    {
        static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(GetAssembly()).Run(args);

        private static Assembly GetAssembly() => typeof(Program).GetTypeInfo().Assembly;
    }
}
