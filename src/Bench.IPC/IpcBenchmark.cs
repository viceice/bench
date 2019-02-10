using Bench.IPC.Interop;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System;
using System.IO.Pipes;

namespace Bench.IPC
{
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    public class IpcBenchmark : BenchmarkBase
    {
        private AppDomain _appDomain;
        private Domain _domainInterop, _domainDirect;
        private NamedPipeServerStream _pipeServer;

        protected override int RunCount => 128;

        public override void GlobalCleanup()
        {
            _pipeServer.Dispose();
            AppDomain.Unload(_appDomain);
        }

        public override void GlobalSetup()
        {
            base.GlobalSetup();
            _pipeServer = new NamedPipeServerStream(GetType().FullName + ".Test");
            _appDomain = Domain.Setup(GetType().FullName + ".Test").Start();
            _domainInterop = _appDomain.Create<Domain>();
            _domainDirect = new Domain();

            _pipeServer.WaitForConnection();
        }

        [Benchmark(Baseline = true)]
        public object InteropAppDomain() => Run(() => _domainInterop.Process(_request));

        //[Benchmark]
        //public object InteropDirect() => _domainDirect.Process(_request);

        [Benchmark]
        public object InteropPipe()
            => Run(() => Domain.ProcessPipe(_pipeServer, _request));
    }
}
