using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpServer.Codec
{
    public class CodecBase
    {
        public virtual void Close()
        {
#if DEBUG
            Console.WriteLine($"{nameof(CodecBase)} {nameof(Close)}");
#endif
        }
    }
}
