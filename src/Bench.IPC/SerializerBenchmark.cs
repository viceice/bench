using Bench.IPC.Interop;
using BenchmarkDotNet.Attributes;
using ProtoBuf;
using System.IO;

namespace Bench.IPC
{
    public class SerializerBenchmark : BenchmarkBase
    {
        protected override int RunCount => 512;

        private InteropObject ProtBufInternal()
        {
            using (var pipe = new MemoryStream())
            {
                Serializer.SerializeWithLengthPrefix(pipe, _request, PrefixStyle.Base128);
                pipe.Seek(0, SeekOrigin.Begin);
                return Serializer.DeserializeWithLengthPrefix<InteropObject>(pipe, PrefixStyle.Base128);
            }
        }

        [Benchmark]
        public object ProtoBuf() => Run(ProtBufInternal);
    }
}
