using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestServer
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
            Application.Run(new ServerForm1());
            var t= Server.CloseAsync(TimeSpan.FromSeconds(10));
            Console.WriteLine("closing..");
            t.Wait();            
            Console.WriteLine("ServerClosed. Press any to to exit");
            Console.ReadKey();
        }
        public static void StartServer(IPEndPoint ipep)
        {
            Server = new KcpServer.KCPServer();
            App = new TestApplication();
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

            var t = Server.AsyncStart(sc);

            t.Wait();
        }
        public static KcpServer.KCPServer Server;
        public static TestApplication App;
    }
}
