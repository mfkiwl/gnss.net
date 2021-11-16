namespace Asv.Gnss
{
    public class RtcmV2Message15 : RtcmV2MessageBase
    {
        public const int RtcmMessageId = 15;

        public override ushort MessageId => RtcmMessageId;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            var itemCnt = (PayloadLength * 8) / 36;
            Delays = new IonosphericDelayItem[itemCnt];

            for (var i = 0; i < itemCnt; i++)
            {
                Delays[i] = new IonosphericDelayItem();
                bitIndex += Delays[i].Deserialize(buffer, bitIndex);
            }

            return bitIndex - offsetBits;
        }

        public IonosphericDelayItem[] Delays { get; set; }
    }

    public class IonosphericDelayItem : ISerializable
    {
        public int GetMaxByteSize()
        {
            return 5;
        }

        public uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits;

            var sys = RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;
            NavigationSystem = sys == 0 ? NavigationSystemEnum.SYS_GPS : NavigationSystemEnum.SYS_GLO;
            Prn = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 5); bitIndex += 5;
            if (Prn == 0) Prn = 32;
            IonosphericDelay = RtcmV3Helper.GetBitU(buffer, bitIndex, 14) * 0.001; bitIndex += 14;
            var rateOfChange = RtcmV3Helper.GetBits(buffer, bitIndex, 14); bitIndex += 14;

            if (rateOfChange == -8192)
            {
                IonosphericDelay = double.NaN;
                IonoRateOfChange = double.NaN;
            }
            else
            {
                IonoRateOfChange = rateOfChange * 0.05;
            }
            
            return bitIndex - offsetBits;
        }

        public NavigationSystemEnum NavigationSystem { get; set; }

        public byte Prn { get; set; }

        /// <summary>
        /// cm
        /// </summary>
        public double IonosphericDelay { get; set; }

        /// <summary>
        /// cm/min
        /// </summary>
        public double IonoRateOfChange { get; set; }
    }
}