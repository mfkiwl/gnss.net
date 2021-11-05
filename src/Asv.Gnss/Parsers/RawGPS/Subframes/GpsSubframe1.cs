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
        }

        public uint WeekNumber { get; set; }
    }
}