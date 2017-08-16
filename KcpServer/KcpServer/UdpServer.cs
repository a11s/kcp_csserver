using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer
{
    public class UdpServer
    {
        public Action<string> debug = (s) =>
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        };

        IOServer server = new IOServer();
        ConnectionManager cm;
        TaskFactory tf = new TaskFactory();
        public Task AsyncStart(ServerConfig sc)
        {

            cm = ConnectionManager.Create(sc.MaxPlayer)
                .SetSysId(sc.SysId)
                .SetApplicationData(sc.AppId)
                .BindApplication(sc.App)
                .SetTimeout(sc.Timeout)
                .SetFiberPool(sc.Fp)
                ;

            var t = server.InitServerAsync(GetServerHandler(cm), sc.Localipep);
            var t2 = t.ContinueWith((a) =>
            {
                if (a.Result == false)
                {
                    debug("init error");
                }
                else
                {
                    tf.StartNew(() => UpdatePeersThreadLoop(cm), TaskCreationOptions.LongRunning);
                }
            }, TaskContinuationOptions.AttachedToParent);

            var t3 = t.ContinueWith((a) =>
            {
                if (a.Result == false)
                {
                    debug("init error");
                }
                else
                {
                    sc.App.Setup();
                }

            }, TaskContinuationOptions.AttachedToParent);
            return t3;
        }

        public Task CloseAsync(TimeSpan closeTimeout)
        {
            var t = tf.StartNew(() => { cm.SyncClose(closeTimeout); });
            var t2 = t.ContinueWith((a) => server.CloseAsync());
            return t2;
        }

        void UpdatePeersThreadLoop(ConnectionManager cm)
        {
            SpinWait sw = new SpinWait();
            while (cm.App.ApplicationRunning)
            {
                cm.CheckTimeout();
                sw.SpinOnce();
            }
        }

        protected virtual DotNetty.Transport.Channels.ChannelHandlerAdapter GetServerHandler(ConnectionManager cm)
        {
            return new UdpServerHandler(cm);
        }
    }
}
