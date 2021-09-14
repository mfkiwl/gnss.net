namespace Asv.Gnss
{
    public class RtcmV3Message1019 : RtcmV3MessageBase
    {
        public const int RtcmMessageId = 1019;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);



            return bitIndex - offsetBits;
        }

        public override ushort MessageId => RtcmMessageId;
    }
}