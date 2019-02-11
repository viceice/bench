using Bench.IPC.Interop;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Google.Protobuf;
using System;
using System.IO;
using System.IO.Pipes;

namespace Bench.IPC
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class IpcBenchmark : BenchmarkBase
    {
        private AppDomain _appDomain;
        private Domain _domainInterop, _domainDirect;
        private NamedPipeServerStream _pipeServer, _pipeGoogle;

        protected override int RunCount => (1 << 10) / DataSize;

        private object InteropGoogleInternal()
            => ProtoBuf.PbInteropObject.Parser.ParseFrom(_domainInterop.Process(_bpPackage.ToByteArray()));

        public override void GlobalCleanup()
        {
            _pipeServer.Dispose();
            _pipeGoogle.Dispose();
            AppDomain.Unload(_appDomain);
        }

        public override void GlobalSetup()
        {
            base.GlobalSetup();
            _pipeServer = new NamedPipeServerStream(GetType().FullName + ".Test");
            _pipeGoogle = new NamedPipeServerStream(GetType().FullName + ".Google");
            _appDomain = Domain.Setup(GetType().FullName).Start();
            _domainInterop = _appDomain.Create<Domain>();
            _domainDirect = new Domain();

            _pipeServer.WaitForConnection();
            _pipeGoogle.WaitForConnection();
        }

        [Benchmark(Baseline = true)]
        public object InteropAppDomain() => Run(() => _domainInterop.Process(_request));

        //[Benchmark]
        //public object InteropDirect() => _domainDirect.Process(_request);

        [Benchmark]
        public object InteropGoogle()
            => Run(InteropGoogleInternal);

        //[Benchmark]
        //public object InteropPipe()
        //    => Run(() => Domain.ProcessPipe(_pipeServer, _request));

        [Benchmark]
        public object InteropPipeGoogle()
           => Run(() => Domain.ProcessPipe(_pipeGoogle, _bpPackage));
    }
}
