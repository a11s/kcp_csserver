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

namespace TestServer
{
    public partial class ServerForm1 : Form
    {
        public ServerForm1()
        {
            InitializeComponent();
        }
        KcpServer.KCPServer server;
        private void button_start_Click(object sender, EventArgs e)
        {
            if (server != null)
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
            server = new KcpServer.KCPServer();
            Program.App = new TestApplication();
            var sysid = "Test".ToCharArray().Select(a => (byte)a).ToArray();
            var appid = "App1".ToCharArray().Select(a => (byte)a).ToArray();

            var sc = KcpServer.ServerConfig.Create()
                .SetSysId(sysid)
                .SetApplicationData(appid)
                .BindApplication(Program.App)
                .SetTimeout(TimeSpan.FromSeconds(10))
                .SetFiberPool(new Utilities.FiberPool(8))
                .SetLocalIpep(ipep)
                .SetMaxPlayer(8)
                ;

            var t = server.AsyncStart(sc);

            t.Wait();

        }
    }
}
