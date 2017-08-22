# kcp_csserver #

用kcp_warpper跟dotnetty的lib做一个简单的kcpserver.  
## 目前已经完成:  
1 dotnetty 的udp服务器  
2 session管理  
3 集成了kcp用于可靠传输  
4 客户端lib  
5 单线程的Server版本.(某些场景不允许开线程)  
7 支持dotnet core 2.0 Ubuntu下测试通过  
## 计划内的尚未完成的工作   
1 unity3d用的Client版本(mono是个坑,)  
2 更多的测试代码  

## 用法


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
        

## 测试图 
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/kcpserver.png'/>  
<img src='https://github.com/a11s/kcp_csserver/raw/master/KcpServer/TestClient/Images/ubuntu.png'/>  

