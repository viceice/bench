using Bench.IPC.ProtoBuf;
using Google.Protobuf;
using ProtoBuf;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Bench.IPC.Interop
{
    internal class Domain : MarshalByRefObject
    {
        private static void Init(string[] args)
        {
            ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    using (var pipe = new NamedPipeClientStream(args[0] + ".Test"))
                    {
                        pipe.Connect();
                        Console.WriteLine("Client connected ...");
                        while (pipe.IsConnected)
                        {
                            var request = Serializer.DeserializeWithLengthPrefix<InteropObject>(pipe, PrefixStyle.Base128);
                            Serializer.SerializeWithLengthPrefix(pipe, new InteropObject { Data = request.Data }, PrefixStyle.Base128);
                            pipe.WaitForPipeDrain();
                        }

                        Console.WriteLine("Client exiting ...");
                    }
                }
                catch { }
            });

            ThreadPool.QueueUserWorkItem((s) =>
            {
                try
                {
                    using (var pipe = new NamedPipeClientStream(args[0] + ".Google"))
                    {
                        pipe.Connect();
                        Console.WriteLine("GoogleClient connected ...");
                        while (pipe.IsConnected)
                        {
                            var request = PbInteropObject.Parser.ParseDelimitedFrom(pipe);
                            var resp = new ProtoBuf.PbInteropObject { ContentType = request.ContentType, Data = request.Data };
                            resp.WriteDelimitedTo(pipe);
                            pipe.WaitForPipeDrain();
                        }

                        Console.WriteLine("GoogleClient exiting ...");
                    }
                }
                catch { }
            });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void Processing()
        {
            //Thread.Sleep(50);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static PbInteropObject ProcessPipe(NamedPipeServerStream pipe, PbInteropObject req)
        {
            try
            {
                if (!pipe.IsConnected)
                    throw new InvalidOperationException("Missing pipe client");
                req.WriteDelimitedTo(pipe);
                pipe.WaitForPipeDrain();
                return PbInteropObject.Parser.ParseDelimitedFrom(pipe);
            }
            finally
            {
                Processing();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static InteropObject ProcessPipe(PipeStream pipe, InteropObject request)
        {
            try
            {
                if (!pipe.IsConnected)
                    throw new InvalidOperationException("Missing pipe client");
                Serializer.SerializeWithLengthPrefix(pipe, request, PrefixStyle.Base128);
                pipe.WaitForPipeDrain();
                return Serializer.DeserializeWithLengthPrefix<InteropObject>(pipe, PrefixStyle.Base128);
            }
            finally
            {
                Processing();
            }
        }

        public static AppDomainSetup Setup(string pipe)
        {
            var cs = AppDomain.CurrentDomain.SetupInformation;
            var id = Guid.NewGuid();

            var setup = new AppDomainSetup
            {
                ApplicationName = id.ToString(),
                ApplicationBase = cs.ApplicationBase,
                LoaderOptimization = LoaderOptimization.MultiDomainHost,   // Ensures non-domain-neutral assembly loading (except mscorlib)
                DisallowCodeDownload = cs.DisallowCodeDownload,
                AppDomainInitializer = new AppDomainInitializer(Init),
                AppDomainInitializerArguments = new[] { pipe }
            };

            var at = cs.ApplicationTrust;
            if (at != null)
                setup.ApplicationTrust = at;
            return setup;
        }

        public override object InitializeLifetimeService() => null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public InteropObject Process(InteropObject request)
        {
            try
            {
                return new InteropObject { Data = request.Data };
            }
            finally
            {
                Processing();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public byte[] Process(byte[] request)
        {
            try
            {
                var p = PbInteropObject.Parser.ParseFrom(request);
                var res = new PbInteropObject { ContentType = p.ContentType, Data = p.Data };
                return res.ToByteArray();
            }
            finally
            {
                Processing();
            }
        }
    }
}
