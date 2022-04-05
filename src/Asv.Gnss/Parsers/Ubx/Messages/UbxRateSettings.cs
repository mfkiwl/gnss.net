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

        public static byte[] SetRate(double rateHz)
        {
            var msg = new UbxRateSettings { RateHz = rateHz };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        public TimeSystemEnum TimeSystem { get; set; } = TimeSystemEnum.Gps;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 6;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset;

            var rateHz = BitConverter.GetBytes(GetRate(RateHz));
            buffer[bitIndex++] = rateHz[0];
            buffer[bitIndex++] = rateHz[1];

            var navRate = BitConverter.GetBytes(NavRate);
            buffer[bitIndex++] = navRate[0];
            buffer[bitIndex++] = navRate[1];

            var timeSystem = BitConverter.GetBytes((ushort)TimeSystem);
            buffer[bitIndex++] = timeSystem[0];
            buffer[bitIndex++] = timeSystem[1];

            return bitIndex - offset;
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
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            RateHz = 1000.0 / BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            NavRate = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            TimeSystem = (TimeSystemEnum)BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;

            return bitIndex - offsetBits;
        }
    }
}