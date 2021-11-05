using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asv.Gnss
{
    public static class GlonassRawHelper
    {
        public static byte GetWordId(uint[] navBits)
        {
            if (navBits.Length != 3) throw new Exception($"Length of {nameof(navBits)} array must be 3 u32 word (as GLONASS ICD word length)");
            if (navBits[0] >> 31 != 0) throw new Exception("Bits 85 must be 0 (as GLONASS ICD superframe structure)");
            return (byte)((navBits[0] >> 27) & 0xF); // 27 bits offset, 4 bit
        }

        public static uint GetBitU(byte[] buff, uint pos, uint len)
        {
            uint bits = 0;
            uint i;
            for (i = pos; i < pos + len; i++)
                bits = (uint)((bits << 1) + ((buff[i / 8] >> (int)(7 - i % 8)) & 1u));
            return bits;
        }
    }
}
