namespace Asv.Gnss
{
    public class RtcmV2Message21 : RtcmV2MessageBase
    {
        public const int RtcmMessageId = 21;

        public override ushort MessageId => RtcmMessageId;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);



            return bitIndex - offsetBits;
        }
    }
}