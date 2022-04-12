namespace Asv.Gnss
{
    public class UbxNak : UbxMessageBase
    {
        public override byte Class => 0x05;
        public override byte SubClass => 0x00;

        public byte AckClassId { get; set; }
        public byte AckSubclassId { get; set; }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            AckClassId = buffer[byteIndex]; byteIndex++;
            AckSubclassId = buffer[byteIndex]; byteIndex++;

            return byteIndex * 8 - offsetBits;
        }
    }
}