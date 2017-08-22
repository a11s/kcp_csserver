using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.MakeTestBuff;

namespace KcpServer.Lite
{
    public class TestPeer : KcpServer.Lite.PeerBase
    {
        public TestPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(TestPeer)} ctor");
        }
        public override void OnOperationRequest(byte[] data)
        {
            var i = BitConverter.ToInt64(data, 0);
            Console.Write($"{this.Context.SessionId}rec:{i}");
            Fiber.Enqueue(() =>
                     {
                         var snd = i + 1;
                         Console.WriteLine($" snd:{snd}");
                         SendOperationResponse(BitConverter.GetBytes(snd));
                     }
                     );
        }

        public override void OnDisconnect(DateTime lastPackTime, TimeSpan t)
        {
            base.OnDisconnect(lastPackTime, t);
            Console.WriteLine($"{nameof(TestPeer)} {nameof(OnDisconnect)}");
        }
    }


    public class TestKcpPeer : KcpServer.Lite.KcpPeerBase
    {
        public TestKcpPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(TestKcpPeer)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            Console.WriteLine($"{nameof(CheckBigBBuff)}={CheckBigBBuff(data)} size:{data.Length}");
            //send back to client
            SendOperationResponse(data);
        }
    }
}
