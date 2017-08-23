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
    public partial class ClientUdp : Form
    {

        k.UdpClient client;
        IPEndPoint localipep;
        IPEndPoint remoteipep;
        public ClientUdp()
        {
            InitializeComponent();
        }

        private void button_init_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }
            client = new k.UdpClient("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "udppeer".ToCharArray().Select(a => (byte)a).ToArray());

            var arr = textBox_remote.Text.Split(":"[0]);
            remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            client.OnOperationResponse = (buf) =>
            {
                var i = BitConverter.ToInt64(buf, 0);
                Console.Write($"rec:{i}");
                Task.Run(() =>
                {
                    var snd = i + 1;
                    Console.WriteLine($"udp snd:{snd}");
                    client.SendOperationRequest(BitConverter.GetBytes(snd));
                }
                        );
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

            client.Connect(remoteipep);
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            client?.Close();
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            client?.SendOperationRequest(BitConverter.GetBytes((UInt64)1));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            client?.Service();
        }
    }
}
