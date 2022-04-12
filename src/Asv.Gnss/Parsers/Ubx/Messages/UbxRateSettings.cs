using System;

namespace Asv.Gnss
{
    public class UbxRateSettings : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x08;

        public double RateHz { get; set; } = 1.0;
        public ushort NavRate { get; set; } = 1;
        public enum TimeSystemEnum
        {
            Utc = 0,
            Gps = 1,
            Glonass = 2,
            BeiDou = 3,
            Galileo = 4
        }

        public TimeSystemEnum TimeSystem { get; set; } = TimeSystemEnum.Gps;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 6;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            var rateHz = BitConverter.GetBytes(GetRate(RateHz));
            buffer[byteIndex++] = rateHz[0];
            buffer[byteIndex++] = rateHz[1];

            var navRate = BitConverter.GetBytes(NavRate);
            buffer[byteIndex++] = navRate[0];
            buffer[byteIndex++] = navRate[1];

            var timeSystem = BitConverter.GetBytes((ushort)TimeSystem);
            buffer[byteIndex++] = timeSystem[0];
            buffer[byteIndex++] = timeSystem[1];

            return byteIndex - offsetBytes;
        }

        private ushort GetRate(double rateHz)
        {
            var result = rateHz <= 0.0152613506295307 ? (ushort)65525 : (ushort)Math.Round(1000.0 / rateHz);
            
            if (result <= 25) return 25;
            
            var multiplicity = (ushort)(result % 25);
            if (multiplicity <= 12) return (ushort)(result - multiplicity);
            return (ushort)(result + (25 - multiplicity));
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            RateHz = 1000.0 / BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            NavRate = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            TimeSystem = (TimeSystemEnum)BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;

            return byteIndex * 8 - offsetBits;
        }
    }
}