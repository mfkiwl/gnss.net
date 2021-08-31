namespace Asv.Gnss
{
    public static class BitOperationHelper
    {
        public static uint GetBitU(byte[] buff, uint pos, uint len)
        {
            uint bits = 0;
            uint i;
            for (i = pos; i < pos + len; i++)
                bits = (uint)((bits << 1) + ((buff[i / 8] >> (int)(7 - i % 8)) & 1u));
            return bits;
        }

        public static void SetBitU(byte[] buff, uint pos, uint len, double data)
        {
            SetBitU(buff, pos, len, (uint)data);
        }

        public static void SetBitU(byte[] buff, uint pos, uint len, uint data)
        {
            var mask = 1u << (int)(len - 1);

            if (len <= 0 || 32 < len) return;

            for (var i = pos; i < pos + len; i++, mask >>= 1)
            {
                if ((data & mask) > 0)
                    buff[i / 8] |= (byte)(1u << (int)(7 - i % 8));
                else
                    buff[i / 8] &= (byte)(~(1u << (int)(7 - i % 8)));
            }
        }

        public static int GetBits(byte[] buff, uint pos, uint len)
        {
            var bits = GetBitU(buff, pos, len);
            if (len <= 0 || 32 <= len || !((bits & (1u << (int)(len - 1))) != 0))
                return (int)bits;
            return (int)(bits | (~0u << (int)len)); /* extend sign */
        }

        /// <summary>
        /// Carrier-phase - Pseudorange in cycle
        /// </summary>
        /// <param name="cp">carrier-phase</param>
        /// <param name="pr_cyc">pseudorange in cycle</param>
        /// <returns></returns>
        public static double CarrierPhasePseudorange(double cp, double pr_cyc)
        {
            var x = (cp - pr_cyc + 1500.0) % 3000.0;
            if (x < 0)
                x += 3000;
            x -= 1500.0;
            return x;
        }

        public static double getbits_38(byte[] buff, uint pos)
        {
            return GetBits(buff, pos, 32) * 64.0 + GetBitU(buff, pos + 32, 6);
        }
    }
}