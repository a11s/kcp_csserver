using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KcpServer
{
    public class KcpSetting
    {
        public int RecWindowSize { get; set; } = 128;
        public int SndWindowSize { get; set; } = 128;
        /// <summary>
        /// 是否启用 nodelay模式，0不启用；1启用。
        /// </summary>
        public int NoDelay { get; set; } = 1;

        /// <summary>
        /// 协议内部工作的 interval，单位毫秒，比如 10ms或者 20ms
        /// </summary>
        public int NoDelayInterval { get; set; } = 10;
        /// <summary>
        /// 重发跨越包数量,2表示两次ack跨越直接重新传
        /// </summary>
        public int NoDelayResend { get; set; } = 2;
        /// <summary>
        /// 是否启用流量控制
        /// </summary>
        public int NoDelayNC { get; set; } = 1;
        //public int NoDelay { get; set; } = 1;
        /// <summary>
        /// 检测丢包的时间间隔,RTO时都有最小 RTO的限制，即便计算出来RTO为40ms，由于默认的 RTO是100ms，协议只有在100ms后才能检测到丢包，快速模式下为30ms，可以手动更改该值
        /// </summary>
        public int RTO { get; set; } = 100;



        private KcpSetting()
        {

        }

        static KcpSetting _setting = new KcpSetting();

        public static KcpSetting Default
        {
            get
            {
                return _setting;
            }
        }

    }
}
