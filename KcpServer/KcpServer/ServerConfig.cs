using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer
{
    public class ServerConfig
    {
        private byte[] _sysId;
        private byte[] _appId;
        private ApplicationBase _app;
        private TimeSpan _timeout;
        private FiberPool _fp;
        private IPEndPoint _localipep;
        private int _maxPlayer = 20;

        private bool _blocking = false;

        public byte[] SysId { get => _sysId;/* set => sysId = value;*/ }
        public byte[] AppId { get => _appId; /*set => _appId = value;*/ }
        public ApplicationBase App { get => _app; /*set => _app = value;*/ }
        public TimeSpan Timeout { get => _timeout; /*set => _timeout = value;*/ }
        public FiberPool Fp { get => _fp; /*set => _fp = value; */}
        public IPEndPoint Localipep { get => _localipep; /*set => _localipep = value;*/ }
        internal int MaxPlayer { get => _maxPlayer; /*set => maxPlayer = value; */}
        [Obsolete("无效属性,用于兼容Lite")]
        public bool Blocking { get => _blocking; }
        public ServerConfig SetSysId(byte[] _4BytesId)
        {
            this._sysId = _4BytesId;
            return this;
        }

        public ServerConfig SetApplicationData(byte[] appId)
        {
            this._appId = appId;
            return this;
        }

        public ServerConfig BindApplication(ApplicationBase app)
        {
            this._app = app;
            return this;
        }

        public ServerConfig SetTimeout(TimeSpan timeSpan)
        {
            this._timeout = timeSpan;
            return this;
        }

        public ServerConfig SetFiberPool(FiberPool fp)
        {
            this._fp = fp;
            return this;
        }

        public ServerConfig SetLocalIpep(IPEndPoint localipep)
        {
            this._localipep = localipep;
            return this;
        }


        public static ServerConfig Create()
        {
            return new ServerConfig();
        }

        public ServerConfig SetMaxPlayer(int maxplayer)
        {
            this._maxPlayer = maxplayer;
            return this;
        }
        [Obsolete("无效属性,用于兼容Lite")]
        public ServerConfig SetBlocking(bool blocking)
        {
            this._blocking = blocking;
            return this;
        }

        private ServerConfig()
        {

        }
    }
}
