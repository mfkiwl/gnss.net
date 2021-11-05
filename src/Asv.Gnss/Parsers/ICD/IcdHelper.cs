using System;
using Newtonsoft.Json.Linq;

namespace Asv.Gnss
{
    public static class IcdHelper
    {
        public static uint GetBitUReverse(byte[] buff, uint pos, uint len)
        {
            uint bits = 0;
            for (var i = (int)(pos + len) - 1; i >= pos; i--)
                bits = (uint)((bits << 1) + ((buff[i / 8] >> 7 - i % 8) & 1u));
            return bits;
        }

        public static byte[] GetGpsRawSubFrame(byte[] buffer, uint offsetBits, out uint tow, out byte sfNum)
        {
            byte sync = 0x8B;
            var bitIndex = offsetBits + 6;

            

            var preamb = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex +16 , 8); bitIndex += 24 + 2 + 6;
            // if (preamb != sync) throw new Exception($"Preamb = 0x{preamb:X2}. Sync = 0x{sync:X2}");
            tow = RtcmV3Helper.GetBitU(buffer, bitIndex, 17); bitIndex += 17 + 2;
            sfNum = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 3); bitIndex += 3 + 2 + 2; // 64 bit
            var result = new byte[24];
            for (var i = 0; i < 8; i++)
            {
                bitIndex += 6;
                result[i * 3] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
                result[i * 3 + 1] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
                result[i * 3 + 2] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
                bitIndex += 2;
            }

            return result;
        }

        public static byte[] GetGlonassRawSubFrame(byte[] buffer, uint offsetBits, out uint tow, out byte sfNum)
        {
            byte sync = 0x8B;
            var bitIndex = offsetBits + 6;
            var preamb = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 24 + 2 + 6;
            if (preamb != sync) throw new Exception($"Preamb = 0x{preamb:X2}. Sync = 0x{sync:X2}");
            tow = RtcmV3Helper.GetBitU(buffer, bitIndex, 17); bitIndex += 17 + 2;
            sfNum = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 3); bitIndex += 3 + 2 + 2; // 64 bit
            var result = new byte[24];
            for (var i = 0; i < 8; i++)
            {
                bitIndex += 6;
                result[i * 3] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
                result[i * 3 + 1] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
                result[i * 3 + 2] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
                bitIndex += 2;
            }

            return result;
        }
    }
}
