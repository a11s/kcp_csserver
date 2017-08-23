using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Utilities
{
    public enum ClientErrorCode
    {
        BAD_SYSID = -1,
        APP_REFUSED = -2,
        APP_REFUSED2 = -3,
        SERVER_REFUSED = -10,
        SERVER_TIMEOUT = -11,
    }

    public class PackSettings
    {
        public const int MAX_DATA_LEN = 1000;
        public const int HEADER_LEN = 12;
        public const int MAX_RECBUFF_LEN = 1024 * 1024 * 2;//2MB
    }

    public enum PackType
    {
        Udp = 0,
        Kcp = 1,
    }

}
