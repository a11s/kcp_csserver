using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using k = KcpClient;
using System.Runtime.InteropServices;

namespace TestClient
{
    public unsafe partial class ClientForm1 : Form
    {
        public ClientForm1()
        {
            InitializeComponent();
        }
        #region MyRegion

        k.UdpClient kcpclient;
        IPEndPoint localipep;
        IPEndPoint remoteipep;
        #endregion

        private void button_init_Click(object sender, EventArgs e)
        {
            //if (kcpclient == null)
            {
                kcpclient = new k.KcpClient("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "testpeer".ToCharArray().Select(a => (byte)a).ToArray());
            }
            var userid = uint.Parse(textBox_sid.Text);
            var arr = textBox_local.Text.Split(":"[0]);
            localipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            arr = textBox_remote.Text.Split(":"[0]);
            remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            kcpclient.OnOperationResponse = (buf) =>
            {
                var i = BitConverter.ToInt64(buf, 0);

                Console.Write($"rec:{i}");

                Task.Run(
                    () =>
                    {
                        //System.Threading.Thread.Sleep(1000);//过一秒以后发送返回
                        var snd = i + 1;
                        Console.WriteLine($" snd:{snd}");
                        kcpclient.SendOperationRequest(BitConverter.GetBytes(snd));
                    }
                    );
            };
            kcpclient.OnConnected = (sid) =>
            {
                this.Invoke(
                    new Action(() =>
                    {
                        this.Text = sid.ToString();
                    })
                    );

            };

            kcpclient.Connect(remoteipep);
        }






        private void timer1_Tick(object sender, EventArgs e)
        {
            kcpclient?.Service();
        }

        #region Update vars
        EndPoint ipep = new IPEndPoint(0, 0);
        byte[] b = new byte[1400];
        byte[] kb = new byte[1400];

        #endregion

        /// <summary>
        /// senddata
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //kcpclient.SendOperationRequest(Encoding.UTF8.GetBytes("烫烫烫烫烫"));

            kcpclient.SendOperationRequest(BitConverter.GetBytes((UInt64)1));
        }
    }
}
