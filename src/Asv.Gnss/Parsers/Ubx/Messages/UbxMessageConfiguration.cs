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

        public override byte[] GenerateRequest()
        {
            return UbxHelper.GenerateRequest(Class, SubClass, new []{MsgClass, MsgId});
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset;

            buffer[bitIndex++] = MsgClass;
            buffer[bitIndex++] = MsgId;

            return bitIndex - offset;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            MsgClass = buffer[bitIndex++];
            MsgId = buffer[bitIndex++];

            return bitIndex - offsetBits;
        }
    }

    public class UbxMessageConfiguration : UbxMessageConfigurationRequest
    {
        public byte[] Rate { get; set; }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + (Rate?.Length ?? 6);
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset + base.InternalSerialize(buffer, offset);

            foreach (var rate in Rate)
            {
                buffer[bitIndex++] = rate;
            }

            return bitIndex - offset;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            Rate = new byte[PayloadLength - 2];
            for (var i = 0; i < Rate.Length; i++)
            {
                Rate[i] = buffer[bitIndex++];
            }

            return bitIndex - offsetBits;
        }
    }

}