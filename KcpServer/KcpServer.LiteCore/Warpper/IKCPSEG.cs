using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace kcpwarpper
{
    public unsafe struct IQUEUEHEAD
    {
        public IQUEUEHEAD* next;
        public IQUEUEHEAD* prev;
    };
    //=====================================================================
    // SEGMENT
    //=====================================================================
    public unsafe struct IKCPSEG
    {
        public IQUEUEHEAD node;
        public uint conv;
        public uint cmd;
        public uint frg;
        public uint wnd;
        public uint ts;
        public uint sn;
        public uint una;
        public uint len;
        public uint resendts;
        public uint rto;
        public uint fastack;
        public uint xmit;
        //char data[1];
        fixed byte data[1];
    };


    //---------------------------------------------------------------------
    // IKCPCB
    //---------------------------------------------------------------------
    public unsafe struct IKCPCB
    {
        public uint conv, mtu, mss, state;
        public uint snd_una, snd_nxt, rcv_nxt;
        public uint ts_recent, ts_lastack, ssthresh;
        //IINT32 rx_rttval, rx_srtt, rx_rto, rx_minrto;
        public int rx_rttval, rx_srtt, rx_rto, rx_minrto;
        public uint snd_wnd, rcv_wnd, rmt_wnd, cwnd, probe;
        public uint current, interval, ts_flush, xmit;
        public uint nrcv_buf, nsnd_buf;
        public uint nrcv_que, nsnd_que;
        public uint nodelay, updated;
        public uint ts_probe, probe_wait;
        public uint dead_link, incr;
        public IQUEUEHEAD snd_queue;
        public IQUEUEHEAD rcv_queue;
        public IQUEUEHEAD snd_buf;
        public IQUEUEHEAD rcv_buf;
        public uint* acklist;
        public uint ackcount;
        public uint ackblock;
        public void* user;
        public char* buffer;
        public int fastresend;
        public int nocwnd, stream;
        public int logmask;
        //int (* output) (const char* buf, int len, struct IKCPCB *kcp, void* user);
        //void (* writelog) (const char* log, struct IKCPCB *kcp, void* user);
        /// <summary>
        /// <para>if func from dll</para>
        /// <para>IntPtr dll = LoadLibrary("xxx.dll");  </para>
        /// <para>IntPtr func = GetProcAddress(dll, "funcname");</para>
        /// <para>d_output output = (d_output)Marshal.GetDelegateForFunctionPointer(func, typeof(d_output));  </para>
        /// <para>else </para>
        /// <para>IKCPCB ikcpcb;</para>
        /// <para>d_writelog act = (a, b, c) => { };</para>
        /// <para>ikcpcb.writelog = Marshal.GetFunctionPointerForDelegate(act);        </para>
        /// <para> output = Marshal.GetFunctionPointerForDelegate(new k.d_output(udp_output));</para>
        /// </summary>
        public IntPtr output;
        public IntPtr writelog;
    };
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate int d_output(byte *buf, int len, IKCPCB* kcp, void* user);
    [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
    public unsafe delegate void d_writelog(byte* log, IKCPCB* kcp, void* user);

    #region interface

    #endregion



    unsafe class pp
    {
        unsafe void p()
        {
            IKCPCB ikcpcb;
            d_writelog act = (a, b, c) => { };
            ikcpcb.writelog = Marshal.GetFunctionPointerForDelegate(act);
        }
    }
}
