using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KcpClient
{
    public class KcpClient : UdpClient
    {
        public KcpClient(byte[] arr, int sid, byte[] appData) : base(arr, sid, appData)
        {

        }

        protected override void ProcessIncomingData(byte[] data)
        {
            base.ProcessIncomingData(data);
        }

        protected override void ProcessOutgoingData(byte[] buff)
        {
            base.ProcessOutgoingData(buff);
        }
    }
}
