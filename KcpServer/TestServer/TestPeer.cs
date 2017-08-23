using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KcpServer;
using static Utilities.MakeTestBuff;
namespace TestServer
{
    class TestUdpPeer : KcpServer.PeerBase
    {
        public TestUdpPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(TestUdpPeer)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            //todo something
            //for (int k = 0; k < 1000000000; k++)
            //{
            //    Math.Sqrt(k);
            //}
            var i = BitConverter.ToInt64(data, 0);
            var snd = i + 1;
            this.SendOperationResponse(BitConverter.GetBytes(snd));
            Console.WriteLine($"sid:{this.SessionId}->rec:{i} snd:{snd}");
        }
    }
    public class BigBuffPeer : KcpServer.KcpPeerBase
    {
        public BigBuffPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(BigBuffPeer)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            Console.WriteLine($"{nameof(CheckBigBBuff)}={CheckBigBBuff(data)} size:{data.Length}");
            //send back to client
            SendOperationResponse(data);
        }
    }

    public class ExPeer : KcpPeerEx
    {
        public ExPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(ExPeer)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            //貌似这里不知道是不是非可靠消息传递过来的，没有暴露到接口层面。不过貌似意义不大，反正收到了指令，在乎这个指令是怎么来的吗？
            if (data.Length==sizeof(UInt64))
            {
                var i = BitConverter.ToInt64(data, 0);
                var snd = i + 1;
                this.SendOperationResponse(BitConverter.GetBytes(snd),true);
                Console.WriteLine($"sid:{this.SessionId}->rec:{i} snd:{snd}");
            }
            else
            {
                Console.WriteLine($"{nameof(CheckBigBBuff)}={CheckBigBBuff(data)} size:{data.Length}");
                //send back to client
                SendOperationResponse(data);
            }
        }
    }
}
