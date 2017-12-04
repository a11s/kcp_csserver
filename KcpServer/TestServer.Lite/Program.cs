using KcpServer.Lite;
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
            Console.WriteLine("All test:");
            Console.WriteLine("1 PureUdp test");
            Console.WriteLine("2 PureKcp test");
            Console.WriteLine("3 Udp+Kcp mix test");
            Console.WriteLine("other: exit");
            string input = "";
            Console.WriteLine("input 1~3:");
            input = Console.ReadLine();
            switch (input.Trim())
            {
                case "0":
                    StartServer = StartServer0;
                    break;
                case "1":
                    StartServer = StartServer1;
                    break;
                case "2":
                    StartServer = StartServer2;
                    break;
                case "3":
                    StartServer = StartServer3;
                    break;
                default:
                    return;
                    break;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            var servertype = Server.GetType().Name;
            Server?.Close(TimeSpan.FromSeconds(10));
            Console.WriteLine("closing..");
            
            Console.WriteLine($"Server {servertype} Closed. Press any to to exit");
            Console.ReadKey();
        }

        public static Action<IPEndPoint> StartServer = (ipep) => { };
        public static void StartServer0(IPEndPoint ipep)
        {
            Server = new KcpServerExLite();
            App = new TestApplication();
            var sysid = "Test".ToCharArray().Select(a => (byte)a).ToArray();
            var appid = "App1".ToCharArray().Select(a => (byte)a).ToArray();

            var sc = ServerConfig.Create()
                .SetSysId(sysid)
                .SetApplicationData(appid)
                .BindApplication(Program.App)
                .SetTimeout(TimeSpan.FromSeconds(10))
                //.SetFiberPool(new Utilities.FiberPool(8))
                .SetLocalIpep(ipep)
                .SetMaxPlayer(8)
                .SetBlocking(true)
                ;
            Server.Start(sc);
            
        }
        public static void StartServer1(IPEndPoint ipep)
        {
            Server = new UdpServerLite();
            App = new TestApplication();
            var sysid = "Test".ToCharArray().Select(a => (byte)a).ToArray();
            var appid = "App1".ToCharArray().Select(a => (byte)a).ToArray();

            var sc = ServerConfig.Create()
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
        public static void StartServer2(IPEndPoint ipep)
        {
            Server = new KcpServerLite();
            App = new TestApplication();
            var sysid = "Test".ToCharArray().Select(a => (byte)a).ToArray();
            var appid = "App1".ToCharArray().Select(a => (byte)a).ToArray();

            var sc = ServerConfig.Create()
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
        public static void StartServer3(IPEndPoint ipep)
        {
            Server = new KcpServerExLite();
            App = new TestApplication();
            var sysid = "Test".ToCharArray().Select(a => (byte)a).ToArray();
            var appid = "App1".ToCharArray().Select(a => (byte)a).ToArray();

            var sc = ServerConfig.Create()
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
        public static TestApplication App;
    }
}
