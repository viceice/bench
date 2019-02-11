using Bench.IPC.Interop;
using Bench.IPC.ProtoBuf;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Google.Protobuf;
using ProtoBuf;
using System.IO;

namespace Bench.IPC
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
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

        private PbInteropObject ProtBufInternalGoogle()
        {
            using (var pipe = new MemoryStream())
            {
                _bpPackage.WriteDelimitedTo(pipe);
                pipe.Seek(0, SeekOrigin.Begin);
                return PbInteropObject.Parser.ParseDelimitedFrom(pipe);
            }
        }

        [Benchmark(Baseline = true)]
        public object ProtoBuf() => Run(ProtBufInternal);

        [Benchmark]
        public object ProtoBufGoogle() => Run(ProtBufInternalGoogle);
    }
}
