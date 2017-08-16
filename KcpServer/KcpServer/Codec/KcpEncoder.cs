using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Codec
{
    public class KcpEncoder:BaseEncoder
    {
        public override byte[] Encode(byte[] data)
        {
            Console.WriteLine("kcp encode");
            return base.Encode(data);
        }

        public override void Close()
        {
            Console.WriteLine("kcp encoder close");
        }
    }
}
