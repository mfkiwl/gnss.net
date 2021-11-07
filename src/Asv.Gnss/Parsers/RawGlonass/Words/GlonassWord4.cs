namespace Asv.Gnss
{
    public class GlonassWord4 : GlonassWordBase
    {
        public override byte WordId => 4;

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            var startBitIndex = 8U;


        }
    }
}