using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asv.Gnss
{
    public class RtcmV2Message14 : RtcmV2MessageBase
    {
        public const int RtcmMessageId = 14;

        
        private static uint AdjustGpsWeek(uint week, uint leap)
        {
            var nowGps = DateTime.UtcNow.AddSeconds(leap);
            var w = 0;
            double second = 0;

            RtcmV3Helper.GetFromTime(nowGps, ref w, ref second);
            if (w < 1560) w = 1560; /* use 2009/12/1 if time is earlier than 2009/12/1 */
            return (uint) (week + (w - week + 1) / 1024 * 1024);
        }

        public override ushort MessageId => RtcmMessageId;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);
            
            var week = RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;
            var hour = RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            var leap = RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex += 6;

            week = AdjustGpsWeek(week, leap);
            GpsTime = RtcmV3Helper.GetFromGps((int)week, hour * 3600.0 + ZCount);
            
            return bitIndex - offsetBits;
        }
    }
}
