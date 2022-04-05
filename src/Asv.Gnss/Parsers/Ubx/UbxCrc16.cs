using System;

namespace Asv.Gnss
{
    public static class UbxCrc16
    {
        public static (byte Crc1, byte Crc2) CalculateCheckSum(byte[] packet, int size, int offset = 2)
        {
            uint a = 0x00;
            uint b = 0x00;
            var i = offset;
            while (i < size)
            {
                a += packet[i++];
                b += a;
            }

            return (Crc1: (byte)(a & 0xFF), Crc2: (byte)(b & 0xFF));
        }
    }
}
