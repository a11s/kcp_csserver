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
        static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        static DateTime lasttime;
        static long counter = 0;
        static long counter2 = 0;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //sw.Start();
            //lasttime = DateTime.Now;
            //System.Threading.SpinWait wait = new System.Threading.SpinWait();
            //for (int i = 0; i < 100000; i++)
            //{
            //    if (DateTime.Now.Subtract(lasttime).TotalMilliseconds > 1000)
            //    {
            //        lasttime = DateTime.Now;
            //        Console.WriteLine($"{sw.ElapsedMilliseconds}\t {counter - counter2}\t {counter}");
            //        counter2 = counter;
            //    }
            //    wait.SpinOnce();
            //    counter++;
            //}
            Console.WriteLine("All test:");
            Console.WriteLine("1 PureUdp test");
            Console.WriteLine("2 PureKcp test");
            Console.WriteLine("3 Udp+Kcp mix test");
            Console.WriteLine("4 PureKcp withflush minrto test");
            Console.WriteLine("other: exit");
            string input = "";
            Console.WriteLine("input 1~4:");
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
                case "4":
                    StartServer = StartServer4;
                    break;
                default:
                    return;
                    break;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerForm1());
            var servertype = Server.GetType().Name;
            var t = Server.AsyncClose(TimeSpan.FromSeconds(10));
            Console.WriteLine("closing..");
            t.Wait();
            Console.WriteLine($"Server {servertype} Closed. Press any to to exit");
            Console.ReadKey();
        }

        public static Action<IPEndPoint> StartServer = (ipep) => { };
        public static void StartServer0(IPEndPoint ipep)
        {
            Server = new KcpServer.KcpServerEx();
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
        public static void StartServer1(IPEndPoint ipep)
        {
            Server = new KcpServer.UdpServer();
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
        public static void StartServer2(IPEndPoint ipep)
        {
            Server = new KcpServer.KcpServer();
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
        public static void StartServer3(IPEndPoint ipep)
        {
            Server = new KcpServer.KcpServerEx();
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
        public static void StartServer4(IPEndPoint ipep)
        {
            KcpServer.KcpSetting.Default.RTO = 1;
            KcpServer.KcpSetting.Default.NoDelay = 1;
            KcpServer.KcpSetting.Default.NoDelayInterval = 1;
            KcpServer.KcpSetting.Default.NoDelayResend = 10;
            KcpServer.KcpSetting.Default.NoDelayNC = 1;
            KcpServer.KcpSetting.Default.RecWindowSize = 1024;
            KcpServer.KcpSetting.Default.SndWindowSize = 1024;
            KcpServer.KcpSetting.Default.MTU = Utilities.PackSettings.MAX_DATA_LEN;


            Server = new KcpServer.KcpServer();
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

        public static KcpServer.UdpServer Server;
        public static TestApplication App;
    }
}
