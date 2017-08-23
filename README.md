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
## 计划内的尚未完成的工作   
1 unity3d用的Client版本(mono是个坑,)  
2 更多的测试代码  
3 性能测试以及优化一些无用的new和copy  

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
        
        

## 测试图 
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/kcpserver.png'/>  
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/ubuntu.png'/>  

