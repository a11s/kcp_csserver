using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KcpServer.Lite;
using System.Net;
using static TestServer.Lite.Program;
namespace TestServer.Lite
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        
        private void button_start_Click(object sender, EventArgs e)
        {
            if (Server != null)
            {
                throw new InvalidOperationException("Already started");
            }
            var arr = textBox1.Text.Split(":"[0]);
            int port = 1000;
            if (arr.Length > 1)
            {
                port = int.Parse(arr[1]);
            }
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(arr[0]), port);
            StartServer(ipep);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Server?.Service();
        }
    }
}
