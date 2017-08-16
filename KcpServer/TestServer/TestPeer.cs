using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KcpServer;

namespace TestServer
{
    class TestPeer : KcpServer.PeerBase
    {
        public TestPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"peer sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            var i = BitConverter.ToInt64(data, 0);
            Console.WriteLine($"sid:{this.SessionId}->{i}");
            this.SendOperationResponse(BitConverter.GetBytes(i + 1));
        }
    }
}
