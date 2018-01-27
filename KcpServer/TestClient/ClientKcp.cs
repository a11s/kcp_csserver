extern alias globalclient;
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
using k = globalclient::KcpClient;
using System.Runtime.InteropServices;
using static Utilities.MakeTestBuff;
using System.Threading;

namespace TestClient
{
    public partial class ClientKcp : Form
    {
        k.KcpClient client;
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

            client.Connect(remoteipep, true);
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
            client?.DoWork();
            client?.Service();
        }
        bool withflush;
        private void button_pingpong_init_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                client.Close();
            }
            client = new k.KcpClient("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "kcppeerflush".ToCharArray().Select(a => (byte)a).ToArray());
            var arr = textBox_remote.Text.Split(":"[0]);
            remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
            client.OnOperationResponse = ReqArrival;
            client.OnConnected = (sid) =>
            {
                this.Invoke(
                    new Action(() =>
                    {
                        this.Text = sid.ToString();
                    })
                    );

            };
            withflush = checkBox_withflush.Checked == true;

            client.Connect(remoteipep, false);
        }
        private byte[] datebin;
        System.Diagnostics.Stopwatch sw;
        long counter = 0;
        long lastcounter = 0;
        private void button_pingpong_loop_Click(object sender, EventArgs e)
        {
            sw = new System.Diagnostics.Stopwatch();
            datebin = BitConverter.GetBytes(DateTime.Now.ToBinary());
            sw.Start();
            lastprinttime = DateTime.Now.AddSeconds(1);
            counter = 1;
            while (true)
            {
                if (client.WaitSend > 1000)
                {
                    Application.DoEvents();
                }
                else
                {

                    client.SendOperationRequest(datebin);
                    //if (withflush)
                    //{
                    //    client.KcpFlush();
                    //}
                }

            }
        }
        DateTime lastprinttime;
        void ReqArrival(byte[] bin)
        {
            counter++;
            if (DateTime.Now.Subtract(lastprinttime).TotalMilliseconds > 1000)
            {
                lastprinttime = lastprinttime.AddSeconds(1);
                Console.WriteLine($"{sw.ElapsedMilliseconds}\t {counter - lastcounter}\t {counter}");
                lastcounter = counter;
            }
        }
    }
}
