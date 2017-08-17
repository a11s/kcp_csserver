using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public static class MakeTestBuff
    {
        //every pack 1000, split 128 packs, without header and resent,it should be less than 120
        const int BUFFSIZE = 120*1000;
        public static byte[] MakeBigBuff()
        {
            byte[] buff = new byte[BUFFSIZE];
            for (int i = 0; i < buff.Length; i++)
            {
                buff[i] = (byte)i;
            }
            return buff;
        }

        public static bool CheckBigBBuff(byte[] buff)
        {
            if (buff.Length != BUFFSIZE)
            {
                return false;
            }
            for (int i = 0; i < buff.Length; i++)
            {
                if (buff[i] != (byte)i) return false;
            }

            return true;
        }
    }
}
