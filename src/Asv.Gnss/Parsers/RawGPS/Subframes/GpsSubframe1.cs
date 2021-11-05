using Newtonsoft.Json;

namespace Asv.Gnss
{
    public class GpsSubframe1: GpsSubframeBase
    {
        
        public override byte SubframeId => 1;

        public override void Deserialize(byte[] dataWithoutParity)
        {
            base.Deserialize(dataWithoutParity);
            var startBitIndex = 24U + 24U;
            WeekNumber = GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 10);
            startBitIndex += 10;
            SatteliteAccuracy = (byte) GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 4);
            startBitIndex += 4;
            SatteliteHealth = (byte) GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 6);
            startBitIndex += 6;
            TGD = ((sbyte) GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 8) * GpsRawHelper.P2_31);
            startBitIndex += 8;
            IODC = (GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 10));
            startBitIndex += 10;
            TOC = (GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 16)) * 16;
            startBitIndex += 16;
            Af2 = ((sbyte) GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 8) * GpsRawHelper.P2_55);
            startBitIndex += 8;
            Af1 = ((sbyte) GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 16) * GpsRawHelper.P2_43);
            startBitIndex += 16;
            Af0 = ((sbyte) GpsRawHelper.GetBitU(dataWithoutParity, startBitIndex, 22) * GpsRawHelper.P2_31);
            startBitIndex += 22;
        }

        public double Af0 { get; set; }
        public double Af1 { get; set; }
        public double Af2 { get; set; }
        public uint TOC { get; set; }
        public uint IODC { get; set; }
        public double TGD { get; set; }
        public byte SatteliteAccuracy { get; set; }
        public byte SatteliteHealth { get; set; }
        public uint WeekNumber { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}