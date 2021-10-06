namespace Asv.Gnss
{
    public class RtcmV2Message1 : RtcmV2MessageBase
    {
        public const int RtcmMessageId = 1;

        public override ushort MessageId => RtcmMessageId;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            var itmCnt = PayloadLength / 5;
            ObservationItems = new DObservationItem[itmCnt];

            for (var i = 0; i < itmCnt; i++)
            {
                var item = new DObservationItem();
                bitIndex += item.Deserialize(buffer, bitIndex);
                ObservationItems[i] = item;
            }
            
            return bitIndex - offsetBits;
        }

        public DObservationItem[] ObservationItems { get; set; }
    }
}