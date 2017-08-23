using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace KcpServer.Lite
{
    public class ConnectionManager
    {
        public const int MAX_CONN_EXCEED = -1;
        private Dictionary<int, PeerBase> ConnDict = new Dictionary<int, PeerBase>();
        public byte[] _SysId = new byte[4];
        public byte[] _ApplicationData = new byte[0];
        ApplicationBase _app;
        private TimeSpan _connectionTimeout = TimeSpan.FromMinutes(1);

        public byte[] SysId { get => _SysId; }
        public byte[] ApplicationData { get => _ApplicationData; }
        public ApplicationBase App { get => _app; /*set => _app = value;*/ }
        public TimeSpan ConnectionTimeout { get => _connectionTimeout; /*set => _connectionTimeout = value;*/ }

        internal void SyncClose(TimeSpan timeSpan)
        {
            App?.Close();
        }



        LinkedList<int> PeerIdPool = null;
        public Action<string> log = (s) => { Console.WriteLine(s); };


        public ConnectionManager BindApplication(ApplicationBase app)
        {
            _app = app;
            _app.SetConnMan(this);
            return this;
        }

        public ConnectionManager SetTimeout(TimeSpan timeSpan)
        {
            this._connectionTimeout = timeSpan;
            return this;
        }



        /// <summary>
        /// 设置 SysId
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public ConnectionManager SetSysId(byte[] buff)
        {
            for (int i = 0; i < _SysId.Length; i++)
            {
                _SysId[i] = buff[i];
            }
            return this;
        }

        List<int> removelist = null;
        internal void CheckTimeout()
        {
            foreach (var item in ConnDict)
            {
                var t = DateTime.Now.Subtract(item.Value.LastPackTime);
                if (t > _connectionTimeout)
                {
                    RecycleSession(item.Key);//归还
                    try
                    {
                        item.Value.OnTimeout(item.Value.LastPackTime, t);
                        //Console.WriteLine($"peer {item.Value.Context.SessionId} timeout@{item.Value.LastPackTime}" );

                    }
                    catch (Exception ex)
                    {
                        log($"sid:{item.Key}, {ex}");
                    }
                    if (removelist == null)
                    {
                        removelist = new List<int>();
                    }
                    removelist.Add(item.Key);

                }
                else
                {
                    //让Peer处理消息
                    item.Value.UpdateInternal();
                }
            }
            if (removelist != null)
            {
                foreach (var item in removelist)
                {
                    ConnDict.Remove(item);
                }
                removelist.Clear();
                removelist = null;
            }
        }

        /// <summary>
        /// 枚举一个空闲的ID
        /// </summary>
        /// <returns></returns>
        public int EnumANewPeerId()
        {
            lock (PeerIdPool)
            {
                if (PeerIdPool.Count > 0)
                {
                    var id = PeerIdPool.First.Value;
                    PeerIdPool.RemoveFirst();
                    return id;
                }
                else
                {
                    return MAX_CONN_EXCEED;
                }
            }
        }

        internal PeerBase FindPeer(int sid)
        {
            if (ConnDict.TryGetValue(sid, out var v))
            {
                return v;
            }
            return null;
        }

        internal void AddConn(PeerBase p)
        {
            ConnDict[p.SessionId] = p;
        }

        public void RecycleSession(int PeerId)
        {
            lock (PeerIdPool)
            {
                PeerIdPool.AddLast(PeerId);
            }
        }
        /// <summary>
        /// 设置建立新连接时候的验证消息
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public ConnectionManager SetApplicationData(byte[] buff)
        {
            if (buff.Length > PackSettings.MAX_DATA_LEN)
            {
                throw new InvalidOperationException("ConnReq too long");
            }
            _ApplicationData = buff;
            return this;
        }

        public static ConnectionManager Create(int MaxConnection)
        {

            var cm = new ConnectionManager();
            cm.PeerIdPool = new LinkedList<int>();
            for (int i = 1; i < MaxConnection + 1; i++)
            {
                cm.PeerIdPool.AddLast(i);
            }
            return cm;
        }


    }
}
