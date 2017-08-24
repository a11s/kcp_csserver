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
using static Utilities.MakeTestBuff;

namespace TestClient
{
    public partial class ClientKcp : Form
    {
        k.UdpClient client;
        IPEndPoint remoteipep;
        public ClientKcp()
        {
            InitializeComponent();
        }

        private void button_init_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }
            client = new k.KcpClient("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "kcppeer".ToCharArray().Select(a => (byte)a).ToArray());
            var arr = textBox_remote.Text.Split(":"[0]);
            remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            client.OnOperationResponse = (buf) =>
                {
                    Console.WriteLine($"{nameof(CheckBigBBuff)}={CheckBigBBuff(buf)} size:{buf.Length} ");
                };
            client.OnConnected = (sid) =>
            {
                this.Invoke(
                    new Action(() =>
                    {
                        this.Text = sid.ToString();
                    })
                    );

            };

            client.Connect(remoteipep,true);
        }

        private void button_Send_Click(object sender, EventArgs e)
        {
            client?.SendOperationRequest(MakeBigBuff());
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            client?.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            client?.Service();
        }
    }
}
