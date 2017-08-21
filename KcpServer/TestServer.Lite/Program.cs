using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestServer.Lite
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            Server?.Close(TimeSpan.FromSeconds(10));
            Console.WriteLine("closing..");
            
            Console.WriteLine("ServerClosed. Press any to to exit");
            Console.ReadKey();
        }


        public static void StartServer(IPEndPoint ipep)
        {
            Server = new KcpServer.Lite.KcpServerLite();
            App = new TestApplicationLite();
            var sysid = "Test".ToCharArray().Select(a => (byte)a).ToArray();
            var appid = "App1".ToCharArray().Select(a => (byte)a).ToArray();

            var sc = KcpServer.Lite.ServerConfig.Create()
                .SetSysId(sysid)
                .SetApplicationData(appid)
                .BindApplication(Program.App)
                .SetTimeout(TimeSpan.FromSeconds(10))
                //.SetFiberPool(new Utilities.FiberPool(8))
                .SetLocalIpep(ipep)
                .SetMaxPlayer(8)
                ;

            Server.Start(sc);

            
        }
        public static KcpServer.Lite.UdpServerLite Server;
        public static TestApplicationLite App;
    }
}
