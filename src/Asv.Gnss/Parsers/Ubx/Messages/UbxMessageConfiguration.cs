namespace Asv.Gnss
{
    public class UbxMessageConfigurationRequest : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x01;

        public byte MsgClass { get; set; }
        public byte MsgId { get; set; }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 2;
        }

        public override GnssMessageBase GetRequest()
        {
            return new UbxMessageConfigurationRequest
            {
                MsgClass = MsgClass,
                MsgId = MsgId
            };
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            buffer[byteIndex++] = MsgClass;
            buffer[byteIndex++] = MsgId;

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            MsgClass = buffer[byteIndex++];
            MsgId = buffer[byteIndex++];

            return byteIndex * 8 - offsetBits;
        }
    }

    public class UbxMessageConfiguration : UbxMessageConfigurationRequest
    {
        public byte[] Rate { get; set; }

        public UbxMessageConfiguration(byte msgClass, byte msgId, byte msgRate)
        {
            MsgClass = msgClass;
            MsgId = msgId;
            Rate = new byte[] { 0, msgRate, 0, msgRate, 0, 0 };
        }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + (Rate?.Length ?? 6);
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes + base.InternalSerialize(buffer, offsetBytes);

            foreach (var rate in Rate)
            {
                buffer[byteIndex++] = rate;
            }

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            Rate = new byte[PayloadLength - 2];
            for (var i = 0; i < Rate.Length; i++)
            {
                Rate[i] = buffer[byteIndex++];
            }

            return byteIndex * 8 - offsetBits;
        }
    }

}