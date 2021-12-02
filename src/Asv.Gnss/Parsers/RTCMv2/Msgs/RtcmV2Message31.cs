using System;

namespace Asv.Gnss
{
    public class RtcmV2Message31 : RtcmV2MessageBase
    {
        protected override DateTime adjhour(double zcnt)
        {
            var time = DateTime.UtcNow;
            double tow = 0;
            var week = 0;
        
            RtcmV3Helper.GetFromTime(time, ref week, ref tow);
        
            var hour = Math.Floor(tow / 3600.0);
            var sec = tow - hour * 3600.0;
            if (zcnt < sec - 1800.0) zcnt += 3600.0;
            else if (zcnt > sec + 1800.0) zcnt -= 3600.0;
        
            return RtcmV3Helper.GetFromGps(week, hour * 3600 + zcnt);
        }

        public const int RtcmMessageId = 31;

        public override ushort MessageId => RtcmMessageId;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            //ZCount += 18;

            var itmCnt = PayloadLength / 5;
            ObservationItems = new DObservationItem[itmCnt];

            for (var i = 0; i < itmCnt; i++)
            {
                var item = new DObservationItem(NavigationSystemEnum.SYS_GLO);
                bitIndex += item.Deserialize(buffer, bitIndex);
                ObservationItems[i] = item;
            }

            return bitIndex - offsetBits;
        }

        public DObservationItem[] ObservationItems { get; set; }
    }
}