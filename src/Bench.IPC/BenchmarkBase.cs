using Bench.IPC.Interop;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bench.IPC
{
    public abstract class BenchmarkBase
    {
        protected InteropObject _request;

        [Params(1 << 10, 1 << 16, 1 << 20)]
        public int DataSize;

        protected virtual int RunCount => 64;

        private byte[] GetData()
        {
            var res = new byte[DataSize];
            new Random().NextBytes(res);
            return res;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected IEnumerable<InteropObject> Run(Func<InteropObject> f)
           => Enumerable.Range(0, RunCount).Select(i => f()).ToArray();

        [GlobalCleanup]
        public virtual void GlobalCleanup()
        {
        }

        [GlobalSetup]
        public virtual void GlobalSetup()
        {
            _request = new InteropObject { Data = GetData() };
        }
    }
}
