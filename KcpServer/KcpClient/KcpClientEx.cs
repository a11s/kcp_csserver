using System;
using System.Collections.Generic;
using kcpwarpper;
using System.Runtime.InteropServices;

namespace KcpClient
{
    public class KcpClientEx : KcpClient
    {
        public KcpClientEx(byte[] arr, int sid, byte[] appData) : base(arr, sid, appData)
        {
        }
        protected override void ProcessIncomingData(byte[] data, int start, int len)
        {
            //if (data.Length > 0)
            if (len > 0)
            {
                var type = (Utilities.PackType)data[start];
                switch (type)
                {
                    case Utilities.PackType.Udp:
                        var realdata = new byte[len - 1];//todo , try to reuse 
                        Array.Copy(data, start + 1, realdata, 0, realdata.Length);
                        if (Incoming != null)
                        {
                            Incoming.Enqueue(realdata);
                        }
                        break;
                    case Utilities.PackType.Kcp:
                        base.ProcessIncomingData(data, start + 1, len - 1);
                        break;
                    default:
                        //unknown packtype, drop it
                        debug($"unknown packtype, drop it. typeid = {type}");
                        break;
                }

            }

        }
        /// <summary>
        /// default is reliable
        /// </summary>
        /// <param name="buff"></param>
        public override void SendOperationRequest(byte[] buff)
        {
            SendOperationRequest(buff, false);
        }
        public virtual void SendOperationRequest(byte[] buff, bool unreliable)
        {
            if (!Connected)
            {
                throw new InvalidOperationException("not connected");
            }
            byte[] newbuff = PackReliable(buff, unreliable);
            ProcessOutgoingData(newbuff, 0, newbuff.Length);
        }

        private static byte[] PackReliable(byte[] buff, bool unreliable)
        {
            var newbuff = new byte[buff.Length + 1];
            byte type;
            if (unreliable)
            {
                type = (byte)Utilities.PackType.Udp;
            }
            else
            {
                type = (byte)Utilities.PackType.Kcp;
            }
            newbuff[0] = type;
            Array.Copy(buff, 0, newbuff, 1, buff.Length);
            return newbuff;
        }

        protected override void ProcessOutgoingData(byte[] buff, int start, int len)
        {
            var type = (Utilities.PackType)buff[start];
            switch (type)
            {
                case Utilities.PackType.Udp:
                    if (Outgoing != null)
                    {
                        if (start == 0)
                        {
                            Outgoing.Enqueue(buff);
                        }
                        else
                        {
                            var buf = new byte[len];
                            Array.Copy(buff, start, buf, 0, len);
                            Outgoing.Enqueue(buf);
                        }
                    }
                    break;
                case Utilities.PackType.Kcp:
                    base.ProcessOutgoingData(buff, start + 1, len - 1);
                    break;
                default:
                    break;
            }
        }

        protected override unsafe int udp_output(byte* buf, int len, IKCPCB* kcp, void* user)
        {
            byte[] buff = new byte[len + 1];
            Marshal.Copy(new IntPtr(buf), buff, 1, len);
#if PRINTPACK
            printpack($"kcp_output:{buff.Length}:{string.Join(",",buff)}");
#endif
            buff[0] = (byte)Utilities.PackType.Kcp;
            if (Outgoing != null)
            {
                Outgoing.Enqueue(buff);
            }
            return 0;
        }
        public override int MaxKcpPackSize => Utilities.PackSettings.MAX_DATA_LEN - 1;
    }
}
