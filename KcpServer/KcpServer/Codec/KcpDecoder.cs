using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static kcpwarpper.KCP;


namespace KcpServer.Codec
{
    public unsafe class KcpDecoder : BaseDecoder
    {
        private PeerContext x;

        public KcpDecoder(PeerContext x)
        {
            this.x = x;
            KcpEncoder.CreateKcp(x);
        }

        public override byte[] Decode(byte[] data)
        {
            Console.WriteLine("kcp decode");
            return data;
        }
        public override void Close()
        {
            if (x.EncoderData != null)
            {
                Console.WriteLine($"release kcp {(int)x.EncoderData}");
                ikcp_release(x.EncoderData);
                x.EncoderData = null;
            }
            Console.WriteLine("kcp decoder close");
        }
    }
}
