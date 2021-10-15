using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Asv.Gnss
{
    /// <summary>
    /// Source https://docs.novatel.com/OEM7/Content/Messages/32_Bit_CRC.htm
    /// </summary>
    public static class SbfCrc16
    {
        public static ushort ComputeCRC16(byte[] bytes, int seedInBytes, int count)
        {
            /*
            Reference : http://www.sunshine2k.de/articles/coding/crc/understanding_crc.html
            Algorithm : CRC-16-CCIT-FALSE
            Polynomial : 0x1021
            Initial Value : 0x0000
            Final XOR Value : 0x0 (Actually 0 has no impact)            
            */
            const ushort generator = 0x1021;    /* divisor is 16bit */
            ushort crc = 0x0000; /* CRC value is 16bit (Initial Value)*/

            for (var index = seedInBytes; index < count; index++)
            {
                var b = bytes[index];
                crc ^= (ushort) (b << 8); /* move byte into MSB of 16bit CRC */

                for (var i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) != 0) /* test for MSB = bit 15 */
                    {
                        crc = (ushort) ((crc << 1) ^ generator);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            return crc;
        }

       

      
        
    }

}