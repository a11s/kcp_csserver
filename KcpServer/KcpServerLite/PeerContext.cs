using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using KcpServer.Lite.Codec;

namespace KcpServer.Lite
{
    public class PeerContext
    {
        public byte[] ApplicationData { get; internal set; }
        public EndPoint RemoteEP { get; internal set; }
        public EndPoint LocalEP { get; internal set; }
        public int SessionId { get; internal set; }
        public ConnectionManager ConnectionManager { get; internal set; }
        public CodecBase Codec { get; internal set; }
    }
}
