namespace Asv.Gnss
{
    public class UbxAck : UbxMessageBase
    {
        public override byte Class => 0x05;
        public override byte SubClass => 0x01;

        public byte AckClassId { get; set; }
        public byte AckSubclassId { get; set; }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex =  offsetBits + base.Deserialize(buffer, offsetBits);

            AckClassId = buffer[bitIndex]; bitIndex++;
            AckSubclassId = buffer[bitIndex]; bitIndex++;

            return bitIndex - offsetBits;
        }
    }
}