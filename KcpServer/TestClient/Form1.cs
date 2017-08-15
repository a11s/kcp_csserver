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
    public unsafe partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        #region MyRegion

        k.KcpClient kcpclient;
        IPEndPoint localipep;
        IPEndPoint remoteipep;
        #endregion

        private void button_init_Click(object sender, EventArgs e)
        {
            //if (kcpclient == null)
            {
                kcpclient = new k.KcpClient("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "App1".ToCharArray().Select(a => (byte)a).ToArray());
            }
            var userid = uint.Parse(textBox_sid.Text);
            var arr = textBox_local.Text.Split(":"[0]);
            localipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            arr = textBox_remote.Text.Split(":"[0]);
            remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            kcpclient.OnOperationResponse = (buf) =>
            {
                Console.WriteLine(Encoding.UTF8.GetString(buf));
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
            kcpclient.SendOperationRequest(Encoding.UTF8.GetBytes("烫烫烫烫烫"));
        }
    }
}
