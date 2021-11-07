namespace Asv.Gnss
{
    public class GlonassWord5 : GlonassWordBase
    {
        public override byte WordId
        {
            get => 5;
            protected set { }
        }

        public override void Deserialize(byte[] data)
        {
            base.Deserialize(data);
            var startBitIndex = 8U;


        }
    }
}