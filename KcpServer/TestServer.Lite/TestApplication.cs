using KcpServer.Lite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer.Lite
{
    public class TestApplication : ApplicationBase
    {
        public override PeerBase CreatePeer(PeerContext peerContext)
        {
            var peertype = Encoding.UTF8.GetString(peerContext.ApplicationData);
            if (peertype == "udppeer")
            {
                return new TestUdpPeer(peerContext);
            }
            else if (peertype == "kcppeer")
            {
                return new BigBuffPeer(peerContext);
            }
            else if (peertype == "mixpeer")
            {
                return new ExPeer(peerContext);
            }
            else if (peertype == "kcppeerflush")
            {
                return new BigBuffPeerFlush(peerContext);
            }
            else
            {
                Console.WriteLine($"NotImplementedPeer");
                return null;
            }
        }

        public override void Setup()
        {
            Console.WriteLine($"{nameof(TestApplication)} {nameof(Setup)} {LocalEndPoint}");
        }

        public override void TearDown()
        {
            Console.WriteLine($"{nameof(TestApplication)} {nameof(TearDown)}");
        }



    }
}
