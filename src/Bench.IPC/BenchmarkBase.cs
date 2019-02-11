using Bench.IPC.Interop;
using Bench.IPC.ProtoBuf;
using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Bench.IPC
{
    public abstract class BenchmarkBase
    {
        protected PbInteropObject _bpPackage;
        protected InteropObject _request;

        [Params(10, 14, 15, 16, 19)]
        public int DataSize;

        protected virtual int RunCount => 512;

        protected byte[] GetData()
        {
            var res = new byte[1 << DataSize];
            new Random().NextBytes(res);
            return res;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected IEnumerable<T> Run<T>(Func<T> f)
           => Enumerable.Range(0, RunCount).Select(i => f()).ToArray();

        [GlobalCleanup]
        public virtual void GlobalCleanup()
        {
        }

        [GlobalSetup]
        public virtual void GlobalSetup()
        {
            Console.WriteLine($"Runs: {RunCount}");
            var data = GetData();
            _request = new InteropObject { Data = data };
            _bpPackage = new PbInteropObject { ContentType = _request.ContentType, Data = ByteString.CopyFrom(data) };
        }
    }
}
