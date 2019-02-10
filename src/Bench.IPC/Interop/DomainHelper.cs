using System;

namespace Bench.IPC.Interop
{
    internal static class DomainHelper
    {
        public static T Create<T>(this AppDomain @this) where T : class
            => (T)@this.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);

        public static AppDomain Start(this AppDomainSetup @this)
                    => AppDomain.CreateDomain(@this.ApplicationName, null, @this);
    }
}
