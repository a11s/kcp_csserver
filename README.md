# kcp_csserver #

用kcp_warpper跟dotnetty的lib做一个简单的kcpserver.  
## 目前已经完成:  
1 dotnetty 的udp服务器  
2 session管理  
3 集成了kcp用于可靠传输  
4 客户端lib  
5 单线程的Server版本.(某些场景不允许开线程)  
7 支持dotnet core 2.0 Ubuntu下测试通过  
8 提供了可靠数据包跟不可靠数据包两种传输方式
9 整理了测试项目，阅读起来更方便了  
10 unity3d用的Client版本可以用了,安卓x64环境下测试通过  
11 新的测试4,大约每秒2W左右.用的WinForm的Timer,防止跨线程问题.
## 计划内的尚未完成的工作    
 更多的测试代码  
 性能测试以及优化一些无用的new和copy  

### 用法 服务器端  


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
        


        
### 用法 客户端  


        private void button_init_Click(object sender, EventArgs e)
        {
                if (client != null)
                {
                        client.Close();
                }
                client = new k.KcpClient("Test".ToCharArray().Select(a => (byte)a).ToArray(), 0, "kcppeer".ToCharArray().Select(a => (byte)a).ToArray());
                var arr = textBox_remote.Text.Split(":"[0]);
                remoteipep = new IPEndPoint(IPAddress.Parse(arr[0]), int.Parse(arr[1]));
                client.OnOperationResponse = (buf) =>
                {
                        Console.WriteLine($"{nameof(CheckBigBBuff)}={CheckBigBBuff(buf)} size:{buf.Length} ");
                };
                client.OnConnected = (sid) =>
                        {
                        this.Invoke(
                                new Action(() =>
                                {
                                        this.Text = sid.ToString();
                                })
                        );

                };
                client.Connect(remoteipep);
        }
        
## 注意事项
1 如果真的需要即时的发出数据,那么需要使用client的flush方法.否则你总会有那么一个小延迟  
2 kcp方法调用是不支持多线程的,如果你使用了flush,那么请不要用后台线程去DoWork().否则不安全.  
3 即便你都是单线程的,也不能无脑的while true去发,这样会把缓冲区爆掉.需要在恰当的时机去看一下waitsnd的值.毕竟带宽不是无限的.  
4 开一个客户端的时候,你的pingpong测试数据可能只有32qps 或者64qps.原因是这种测试正好命中了最糟糕的情况.因为消息的执行并不是收到后立刻执行的,在服务器收到数据以后,先堆积到线程池,每个worker在没有任务的时候会有一个短暂的SpinOnce(),于是这种测试正好全部命中SpinOnce(),这里如果觉得每个用户32qps不够用,就需要修改任务调度这部分.  
5 一定要熟悉kcp的参数设置.要理解每个参数的意义.  

## 测试图 
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/kcpserver.png'/>  
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/ubuntu.png'/>  
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/u3dclient.png'/>  

