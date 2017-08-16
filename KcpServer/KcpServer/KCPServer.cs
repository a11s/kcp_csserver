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
    public class KCPServer
    {
        public Action<string> debug = (s) =>
        {
#if DEBUG
            Console.WriteLine(s);
#endif
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="app">user logic</param>
        /// <param name="_4BytesId">sysid send back to client. eg:"Test".ToArray().Select(a => (byte)a).ToArray() </param>
        /// <param name="AppId">appid send back to client, eg: "App1".ToArray().Select(a => (byte)a).ToArray()</param>
        /// <param name="localipep"></param>
        public Task AsyncStart(ApplicationBase app, byte[] _4BytesId, byte[] AppId, IPEndPoint localipep)
        {
            UdpServer server = new UdpServer();
            FiberPool fp = new FiberPool(2);
            TaskFactory tf = new TaskFactory();
            var cm = ConnectionManager.Create()
                .SetSysId(_4BytesId)
                .SetApplicationData(AppId)
                .BindApplication(app)
                .SetTimeout(TimeSpan.FromSeconds(10))
                .SetFiberPool(fp)
                ;

            var t = server.InitServerAsync(new UdpServerHandler(cm), localipep);
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
                    app.Setup();
                }

            }, TaskContinuationOptions.AttachedToParent);
            return t3;

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
    }
}
