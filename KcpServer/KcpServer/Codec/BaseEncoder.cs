using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Codec
{
    public class BaseEncoder
    {
        public virtual byte[] Encode(byte[] data)
        {
            return data;
        }
        public virtual void Close()
        {

        }
    }
}
