using Bench.IPC.Interop;
using Bench.IPC.ProtoBuf;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Google.Protobuf;
using Newtonsoft.Json;
using ProtoBuf;
using System.IO;

namespace Bench.IPC
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class SerializerBenchmark : BenchmarkBase
    {
        protected override int RunCount => 64;

        private InteropObject JsonInternal()
        {
            var serializer = new JsonSerializer();
            using var pipe = new MemoryStream();
            using (var sw = new StreamWriter(pipe, InteropObject.Utf8, 1024, true))
            using (var jw = new JsonTextWriter(sw))
            {
                serializer.Serialize(jw, _request);
            }
            pipe.Seek(0, SeekOrigin.Begin);
            using var sr = new StreamReader(pipe);
            using var jr = new JsonTextReader(sr);
            return serializer.Deserialize<InteropObject>(jr);
        }

        private InteropObject ProtBufInternal()
        {
            using var pipe = new MemoryStream();
            Serializer.SerializeWithLengthPrefix(pipe, _request, PrefixStyle.Base128);
            pipe.Seek(0, SeekOrigin.Begin);
            return Serializer.DeserializeWithLengthPrefix<InteropObject>(pipe, PrefixStyle.Base128);
        }

        private PbInteropObject ProtBufInternalGoogle()
        {
            using var pipe = new MemoryStream();
            _bpPackage.WriteDelimitedTo(pipe);
            pipe.Seek(0, SeekOrigin.Begin);
            return PbInteropObject.Parser.ParseDelimitedFrom(pipe);
        }

        [Benchmark]
        public object Json() => Run(JsonInternal);

        [Benchmark(Baseline = true)]
        public object ProtoBuf() => Run(ProtBufInternal);

        [Benchmark]
        public object ProtoBufGoogle() => Run(ProtBufInternalGoogle);
    }
}
