using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;


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
            Console.WriteLine(Environment.OSVersion.Platform);
            Console.WriteLine(Environment.OSVersion.VersionString);
            if (Server != null)
            {
                throw new InvalidOperationException("Already started");
            }
            Console.Write("Input ip and port:");
            var str = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(str))
            {
                str = "0.0.0.0:1000";
            }
            var arr = str.Split(":"[0]);
            int port = 1000;
            if (arr.Length > 1)
            {
                port = int.Parse(arr[1]);
            }
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(arr[0]), port);

            StartServer(ipep);
            Console.WriteLine("Press ENTER to close");
            Thread updatethread = new Thread(
                () =>
                {
                    SpinWait sw = new SpinWait();
                    while (App.ApplicationRunning)
                    {
                        Server?.Service();
                        sw.SpinOnce();
                    }
                }
                );
            updatethread.IsBackground = true;
            updatethread.Name = $"{nameof(updatethread)}";
            updatethread.Start();
            while (true)
            {
                var c = Console.ReadKey(false);
                if (c.Key == ConsoleKey.Enter)
                {
                    break;
                }
            }

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
