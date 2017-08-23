using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// 与ServerPackBuilder不同的是,这个支持不可靠消息
    /// </summary>
    public class ServerPackBuilderEx
    {
        byte[] SysIdBuf = new byte[4];
        byte[] SessionIdBuf = new byte[4];
        int SessionId;

        public byte[] GetSysIdBuf() => SysIdBuf;

        public ServerPackBuilderEx(byte a, byte b, byte c, byte d, int sid)
        {
            SysIdBuf[0] = a;
            SysIdBuf[1] = b;
            SysIdBuf[2] = c;
            SysIdBuf[3] = d;
            SessionId = sid;
            FillSessionId();
        }

        public ServerPackBuilderEx(byte[] arr, int sid)
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
            if (len > PackSettings.MAX_DATA_LEN)
            {
                throw new InvalidOperationException($"len is more than {nameof(PackSettings.MAX_DATA_LEN)}:{PackSettings.MAX_DATA_LEN}");
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
            Array.Copy(src, start, dest, PackSettings.HEADER_LEN, len);
            return true;
        }

        internal byte[] MakeHandshakeReturn(int newsid)
        {
            var ret = new byte[PackSettings.HEADER_LEN + sizeof(int)];
            Write(ret, BitConverter.GetBytes(newsid), 0, sizeof(int));
            return ret;
        }

        internal byte[] MakeTimeoutReturn(int errcode, int sid)
        {
            var data = new byte[sizeof(int) + sizeof(int)];
            var dest = new byte[PackSettings.HEADER_LEN + data.Length];
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
                Array.Copy(src, PackSettings.HEADER_LEN, data, 0, len);
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
                Array.Copy(src, PackSettings.HEADER_LEN, data, 0, len);
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
