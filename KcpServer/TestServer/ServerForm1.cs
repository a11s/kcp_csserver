using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static TestServer.Program;
namespace TestServer
{
    public partial class ServerForm1 : Form
    {
        public ServerForm1()
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

        
    }
}
