using System;
using System.Text;

namespace Asv.Gnss
{
    public class RtcmV3Message1029 : RtcmV3MessageBase
    {
        public const int RtcmMessageId = 1029;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            ReferenceStationID = RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12 + 16;
            // ModifiedJulianDay = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 16); bitIndex += 16;
            var secondsOfDDay = RtcmV3Helper.GetBitU(buffer, bitIndex, 17); bitIndex += 17 + 7;
            var dateTime = RtcmV3Helper.GetUtc(DateTime.UtcNow, secondsOfDDay);
            EpochTime = RtcmV3Helper.Utc2Gps(dateTime);
            
            var codeUnitsCount = RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            var buff = new byte[codeUnitsCount];
            for (var i = 0; i < codeUnitsCount; i++)
            {
                buff[i] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            }

            Message = new string(Encoding.UTF8.GetChars(buff));

            return bitIndex - offsetBits;
        }

        public uint ReferenceStationID { get; set; }

        public string Message { get; set; }

        public DateTime EpochTime { get; set; }

        public override ushort MessageId => RtcmMessageId;
    }
}