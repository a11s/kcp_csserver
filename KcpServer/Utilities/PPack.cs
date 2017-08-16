using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Utilities
{
    public enum PackType
    {

    }
    public class ToServerPack
    {
        //前4个字节表示系统,不一样的直接丢弃
        //5~8大于0表示sid,
        //  如果是0表示没有sid,新连接,走握手流程
        //  如果是-1表示断开连接
    }
    public enum ClientErrorCode
    {
        BAD_SYSID = -1,
        APP_REFUSED = -2,
        APP_REFUSED2 = -3,
        SERVER_REFUSED = -10,
        SERVER_TIMEOUT = -11,
    }
    /// <summary>
    /// 不可以超过1000字节,否则用其他Builder替换它
    /// </summary>
    public class ToServerPackBuilder
    {
        public const int MAX_DATA_LEN = 1000;
        public const int HEADER_LEN = 12;
        ///// <summary>
        ///// 错误的SysId,伪造或者端口投递错误
        ///// </summary>
        //public const int BAD_SYSID = -1;
        //public const int APP_REFUSED = -2;
        //public const int APP_REFUSED2 = -3;
        //public const int SERVER_REFUSED = -10;
        //public const int SERVER_TIMEOUT = -11;

        byte[] SysIdBuf = new byte[4];
        byte[] SessionIdBuf = new byte[4];
        int SessionId;

        public byte[] GetSysIdBuf() => SysIdBuf;

        public ToServerPackBuilder(byte a, byte b, byte c, byte d, int sid)
        {
            SysIdBuf[0] = a;
            SysIdBuf[1] = b;
            SysIdBuf[2] = c;
            SysIdBuf[3] = d;
            SessionId = sid;
            FillSessionId();
        }

        public ToServerPackBuilder(byte[] arr, int sid)
        {
            for (int i = 0; i < Math.Min(arr.Length, SysIdBuf.Length); i++)
            {
                SysIdBuf[i] = arr[i];
            }


            SessionId = sid;
            FillSessionId();
        }

        private void FillSessionId()
        {
            var arr = BitConverter.GetBytes(SessionId);
            SessionIdBuf[0] = arr[0];
            SessionIdBuf[1] = arr[1];
            SessionIdBuf[2] = arr[2];
            SessionIdBuf[3] = arr[3];
        }

        public bool Write(byte[] dest, byte[] src, int start, int len)
        {
            if (len > MAX_DATA_LEN)
            {
                return false;
            }
            dest[0] = SysIdBuf[0];
            dest[1] = SysIdBuf[1];
            dest[2] = SysIdBuf[2];
            dest[3] = SysIdBuf[3];
            dest[4] = SessionIdBuf[0];
            dest[5] = SessionIdBuf[1];
            dest[6] = SessionIdBuf[2];
            dest[7] = SessionIdBuf[3];
            var arr = BitConverter.GetBytes(len);
            dest[8] = arr[0];
            dest[9] = arr[1];
            dest[10] = arr[2];
            dest[11] = arr[3];
            //data start @[12]
            Array.Copy(src, start, dest, HEADER_LEN, len);
            return true;
        }

        internal byte[] MakeHandshakeReturn(int newsid)
        {
            var ret = new byte[HEADER_LEN + sizeof(int)];
            Write(ret, BitConverter.GetBytes(newsid), 0, sizeof(int));
            return ret;
        }

        internal byte[] MakeTimeoutReturn(int errcode, int sid)
        {
            var data = new byte[sizeof(int) + sizeof(int)];
            var dest = new byte[HEADER_LEN + data.Length];


            dest[0] = SysIdBuf[0];
            dest[1] = SysIdBuf[1];
            dest[2] = SysIdBuf[2];
            dest[3] = SysIdBuf[3];
            // 0 means handshake or errorcode
            dest[4] = 0;
            dest[5] = 0;
            dest[6] = 0;
            dest[7] = 0;
            // >0 means handshake's sid, <0 means errorcode
            var arr = BitConverter.GetBytes(errcode);
            dest[8] = arr[0];
            dest[9] = arr[1];
            dest[10] = arr[2];
            dest[11] = arr[3];
            // if errorcode ,this is sid
            arr = BitConverter.GetBytes(sid);
            dest[12] = arr[0];
            dest[13] = arr[1];
            dest[14] = arr[2];
            dest[15] = arr[3];
            return dest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="src">input data</param>
        /// <param name="data">msg data</param>
        /// <param name="sid">sessionid,简单鉴别是否伪造.</param>
        /// <param name="sysbuff">if return less than 0,it has data of SysId</param>
        /// <returns></returns>
        public int Read(byte[] src, byte[] data, out int sid, out byte[] sysbuff)
        {
            if (src[0] == SysIdBuf[0] && src[1] == SysIdBuf[1] && src[2] == SysIdBuf[2] && src[3] == SysIdBuf[3])
            {
                sysbuff = null;
                sid = BitConverter.ToInt32(src, 4);
                //这里上层应用保证data足够长
                int len = BitConverter.ToInt32(src, 8);
                Array.Copy(src, HEADER_LEN, data, 0, len);
                return len;
            }
            else
            {
                sysbuff = new byte[4];
                sysbuff[0] = src[0];
                sysbuff[1] = src[1];
                sysbuff[2] = src[2];
                sysbuff[3] = src[3];
                sid = 0;
                return (int)ClientErrorCode.BAD_SYSID;
            }
        }
        public int Read(byte[] src, out byte[] data, out int sid, out byte[] sysbuff)
        {
            if (src[0] == SysIdBuf[0] && src[1] == SysIdBuf[1] && src[2] == SysIdBuf[2] && src[3] == SysIdBuf[3])
            {
                sysbuff = null;
                sid = BitConverter.ToInt32(src, 4);
                //这里上层应用保证data足够长
                int len = BitConverter.ToInt32(src, 8);
                if (len < 0)
                {
                    sid = 0;
                    sysbuff = new byte[4];
                    sysbuff[0] = src[0];
                    sysbuff[1] = src[1];
                    sysbuff[2] = src[2];
                    sysbuff[3] = src[3];
                    data = null;
                    return len;
                }
                data = new byte[len];
                Array.Copy(src, HEADER_LEN, data, 0, len);
                return len;
            }
            else
            {
                sysbuff = new byte[4];
                sysbuff[0] = src[0];
                sysbuff[1] = src[1];
                sysbuff[2] = src[2];
                sysbuff[3] = src[3];
                sid = 0;
                data = null;
                return (int)ClientErrorCode.BAD_SYSID;
            }
        }
    }
}
