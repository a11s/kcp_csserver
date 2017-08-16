using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Codec
{
    public class KcpDecoder : BaseDecoder
    {
        public override byte[] Decode(byte[] data)
        {
            Console.WriteLine("kcp decode");
            return data;
        }
        public override void Close()
        {
            Console.WriteLine("kcp decoder close");
        }
    }
}
