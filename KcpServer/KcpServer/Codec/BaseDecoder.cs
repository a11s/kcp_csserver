using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Codec
{
    public class BaseDecoder
    {
        public virtual byte[] Decode(byte[] data)
        {
            return data;
        }

        public virtual void Close()
        {

        }
    }
}
