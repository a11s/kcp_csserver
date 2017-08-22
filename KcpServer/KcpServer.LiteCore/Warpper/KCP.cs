using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kcpwarpper
{
    public unsafe class KCP
    {

        //---------------------------------------------------------------------
        // interface
        //---------------------------------------------------------------------

        /// <summary>
        /// create a new kcp control object, 'conv' must equal in two endpoint
        /// from the same connection. 'user' will be passed to the output callback
        /// output callback can be setup like this: 'kcp->output = my_udp_output'
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern IKCPCB* ikcp_create(uint conv, void* user);

        /// <summary>
        /// release kcp control object
        /// </summary>
        /// <param name="kcp"></param>
        [DllImport("kcplib")]
        public static extern void ikcp_release(IKCPCB* kcp);

        /// <summary>
        /// set output callback, which will be invoked by kcp
        ///public static extern void ikcp_setoutput(IKCPCB* kcp, int (* output)(byte* buf, int len,             ikcpcb *kcp, void* user));
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="d_output"></param>
        [DllImport("kcplib")]
        public static extern void ikcp_setoutput(IKCPCB* kcp, IntPtr d_output);

        /// <summary>
        ///  user/upper level recv: returns size, returns below zero for EAGAIN
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern int ikcp_recv(IKCPCB* kcp, byte* buffer, int len);
        /// <summary>
        /// user/upper level send, returns below zero for error
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="buffer"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        [DllImport("kcplib")]        
        public static extern int ikcp_send(IKCPCB* kcp, byte* buffer, int len);
        /// <summary>
        /// update state (call it repeatedly, every 10ms-100ms), or you can ask 
        /// ikcp_check when to call it again (without ikcp_input/_send calling).
        /// 'current' - current timestamp in millisec. 
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="current"></param>
        [DllImport("kcplib")]        
        public static extern void ikcp_update(IKCPCB* kcp, uint current);
        /// <summary>
        /// Determine when should you invoke ikcp_update:
        /// returns when you should invoke ikcp_update in millisec, if there 
        /// is no ikcp_input/_send calling. you can call ikcp_update in that
        /// time, instead of call update repeatly.
        /// Important to reduce unnacessary ikcp_update invoking. use it to 
        /// schedule ikcp_update (eg. implementing an epoll-like mechanism, 
        /// or optimize ikcp_update when handling massive kcp connections)
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern uint ikcp_check(IKCPCB* kcp, uint current);
        /// <summary>
        /// when you received a low level packet (eg. UDP packet), call it
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="data"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [DllImport("kcplib")]        
        public static extern int ikcp_input(IKCPCB* kcp, byte* data, long size);
        /// <summary>
        /// flush pending data
        /// </summary>
        /// <param name="kcp"></param>
        [DllImport("kcplib")]        
        public static extern void ikcp_flush(IKCPCB* kcp);
        /// <summary>
        /// check the size of next message in the recv queue
        /// </summary> 
        [DllImport("kcplib")]        
        public static extern int ikcp_peeksize( IKCPCB* kcp);
        /// <summary>
        /// change MTU size, default is 1400
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="mtu"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern int ikcp_setmtu(IKCPCB* kcp, int mtu);

        /// <summary>
        /// set maximum window size: sndwnd=32, rcvwnd=32 by default
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="sndwnd"></param>
        /// <param name="rcvwnd"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern int ikcp_wndsize(IKCPCB* kcp, int sndwnd, int rcvwnd);
        /// <summary>
        /// get how many packet is waiting to be sent
        /// </summary>
        /// <param name="kcp"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern int ikcp_waitsnd(IKCPCB* kcp);

        /// <summary>
        /// fastest: ikcp_nodelay(kcp, 1, 20, 2, 1)
        /// nodelay: 0:disable(default), 1:enable
        /// interval: internal update timer interval in millisec, default is 100ms 
        /// resend: 0:disable fast resend(default), 1:enable fast resend
        /// nc: 0:normal congestion control(default), 1:disable congestion control
        /// </summary>
        /// <param name="kcp"></param>
        /// <param name="nodelay"></param>
        /// <param name="interval"></param>
        /// <param name="resend"></param>
        /// <param name="nc"></param>
        /// <returns></returns>
        [DllImport("kcplib")]
        public static extern int ikcp_nodelay(IKCPCB* kcp, int nodelay, int interval, int resend, int nc);

        [DllImport("kcplib")]
        public static extern void ikcp_log(IKCPCB* kcp, int mask,  byte* fmt, params object[] p);
        /// <summary>
        ///setup allocator
        ///public static extern void ikcp_allocator(void* (* new_malloc)(size_t), void (* new_free)(void*)); //give up 
        /// read conv
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        [DllImport("kcplib")]        
        public static extern uint ikcp_getconv(void* ptr);
    }
}
