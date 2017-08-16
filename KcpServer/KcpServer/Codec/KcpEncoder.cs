using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kcpwarpper.KCP;
namespace KcpServer.Codec
{
    public unsafe class KcpEncoder : BaseEncoder
    {
        private PeerContext x;
        public KcpEncoder(PeerContext x)
        {
            this.x = x;
            CreateKcp(x);

        }

        public static void CreateKcp(PeerContext x)
        {
            if (x.EncoderData == null)
            {
                x.EncoderData = ikcp_create((uint)x.SessionId, (void*)0);
                if ((int)x.EncoderData > 0)
                {
                    Console.WriteLine($"create kcp {(int)x.EncoderData}");
                }
                else
                {
                    throw new NullReferenceException($"kcp create failed {(int)x.EncoderData}");
                }
            }
        }

        public override byte[] Encode(byte[] data)
        {
            Console.WriteLine("kcp encode");
            return base.Encode(data);
        }

        public override void Close()
        {
            if (x.EncoderData != null)
            {
                Console.WriteLine($"release kcp {(int)x.EncoderData}");
                ikcp_release(x.EncoderData);
                x.EncoderData = null;
            }
            Console.WriteLine("kcp encoder close");
        }
    }
}
