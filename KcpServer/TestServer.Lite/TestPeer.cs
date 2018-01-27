using KcpServer.Lite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utilities.MakeTestBuff;
namespace TestServer.Lite
{
    class TestUdpPeer : PeerBase
    {
        public TestUdpPeer(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(TestUdpPeer)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            var i = BitConverter.ToInt64(data, 0);
            var snd = i + 1;
            this.SendOperationResponse(BitConverter.GetBytes(snd));
            Console.WriteLine($"sid:{this.SessionId}->rec:{i} snd:{snd}");
        }
    }

    class TestUdpPeer2 : PeerBase
    {
        public TestUdpPeer2(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(TestUdpPeer2)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            this.SendOperationResponse(data);
        }
    }
    public class BigBuffPeer : KcpPeerBase
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

    public class BigBuffPeerFlush : KcpPeerBase
    {
        public BigBuffPeerFlush(PeerContext pc) : base(pc)
        {
            Console.WriteLine($"{nameof(BigBuffPeerFlush)} sid:{pc.SessionId} created");
        }

        public override void OnOperationRequest(byte[] data)
        {
            if (data.Length > sizeof(UInt64))
            {
                Console.WriteLine($"{nameof(CheckBigBBuff)}={CheckBigBBuff(data)} size:{data.Length}");
            }

            //send back to client
            SendOperationResponse(data);
            KcpFlush();
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
