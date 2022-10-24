using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Bench.IPC.Interop
{
    [ProtoContract]
    [DataContract]
    [Serializable]
    public class InteropObject
    {
        [DataMember]
        [ProtoMember(1)]
        private IDictionary<string, string> _props;

        public static readonly Encoding Utf8 = new UTF8Encoding(false);

        [DataMember]
        [ProtoMember(2)]
        public string ContentType { get; set; }

        [DataMember]
        [ProtoMember(3)]
        public byte[] Data { get; set; }

        public IDictionary<string, string> Props
        {
            get { return _props; }
        }

        public InteropObject()
        {
            ContentType = "text/plain;charset=" + Utf8.WebName;
            _props = new Dictionary<string, string>();
        }
    }
}
