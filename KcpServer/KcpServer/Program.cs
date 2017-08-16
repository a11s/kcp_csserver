using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpServer server = new UdpServer();
            ApplicationBase app = new GWApp();
            FiberPool fp = new FiberPool(2);
            TaskFactory tf = new TaskFactory();
            var cm = ConnectionManager.Create(2)
                .SetSysId("Test".ToArray().Select(a => (byte)a).ToArray())
                .SetApplicationData("App1".ToArray().Select(a => (byte)a).ToArray())
                .BindApplication(app)
                .SetTimeout(TimeSpan.FromSeconds(10))
                .SetFiberPool(fp)
                ;

            var t = server.InitServerAsync(new UdpServerHandler(cm), new System.Net.IPEndPoint(IPAddress.Any, 1000));
            var t2 = t.ContinueWith((a) =>
             {
                 if (a.Result == false)
                 {
                     Console.WriteLine("init error");
                 }
                 else
                 {
                     tf.StartNew(() => CheckTimeoutThreadLoop(cm), TaskCreationOptions.LongRunning);
                 }
             }, TaskContinuationOptions.AttachedToParent);

            var t3 = t.ContinueWith((a) =>
            {
                if (a.Result == false)
                {
                    Console.WriteLine("init error");
                }
                else
                {
                    app.Setup();
                }

            }, TaskContinuationOptions.AttachedToParent);

            t3.Wait();
            Console.WriteLine("Press any key to close");
            Console.ReadKey();
            app.TearDown();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        static void CheckTimeoutThreadLoop(ConnectionManager cm)
        {
            while (cm.App.ApplicationRunning)
            {
                cm.CheckTimeout();
                System.Threading.Thread.Sleep(1000);
            }
        }

        public class GWApp : ApplicationBase
        {
            public override PeerBase CreatePeer(PeerContext peerContext)
            {
                return new PlayerConn(peerContext);
            }

            public override void Setup()
            {
                throw new NotImplementedException();
            }

            public override void TearDown()
            {
                throw new NotImplementedException();
            }
        }

        public class PlayerConn : PeerBase
        {
            public PlayerConn(PeerContext pc) : base(pc)
            {
                Console.WriteLine("player new");
            }

            public override void OnOperationRequest(byte[] data)
            {
                Console.WriteLine($"rec:{this.SessionId}:" + Encoding.UTF8.GetString(data));
                SendOperationResponse(Encoding.UTF8.GetBytes("屯屯屯屯屯"));
            }
        }
    }


}
