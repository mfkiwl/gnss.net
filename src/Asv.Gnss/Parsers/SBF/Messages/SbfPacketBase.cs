using System;

namespace Asv.Gnss
{
   

    public abstract class SbfPacketBase: GnssMessageBaseWithId<ushort>
    {
        public override string ProtocolId => SbfBinaryParser.GnssProtocolId;

        public override ushort MessageId => (ushort) ((MessageType & 0x1fff) + (MessageRevision << 13));

        public abstract ushort MessageRevision { get; }
        public abstract ushort MessageType { get; }

        public override int GetMaxByteSize()
        {
            return ComNavBinaryParser.MaxPacketSize;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var offsetInBytes = (int)(offsetBits / 8);
            var msgId = BitConverter.ToUInt16(buffer, offsetInBytes + 4);
            var msgLength = BitConverter.ToUInt16(buffer, offsetInBytes + 6);
            var messageType = (ushort) (msgId & 0x1fff << 0);
            var messageRevision = (ushort) (msgId >> 13);

            if (messageType != MessageType) throw new GnssParserException(ComNavBinaryParser.GnssProtocolId, $"Error to deserialize SBF packet message. MessageType not equal (want [{MessageType}] read [{messageType}])");
            if (messageRevision != MessageRevision) throw new GnssParserException(ComNavBinaryParser.GnssProtocolId, $"Error to deserialize SBF packet message. Id not equal (want [{MessageRevision}] read [{messageRevision}])");
            if (msgId != MessageId) throw new GnssParserException(ComNavBinaryParser.GnssProtocolId, $"Error to deserialize SBF packet message. Id not equal (want [{MessageId}] read [{msgId}])");

            TOW = BitConverter.ToUInt32(buffer, offsetInBytes + 8);
            WNc = BitConverter.ToUInt16(buffer, offsetInBytes + 12);

            UtcTime = new DateTime(1980,1,06).AddMilliseconds(TOW + WNc * 604800000 /* ms in 1 week */);
            DeserializeMessage(buffer, offsetBits + 14 * 8U);
            return (4U + msgLength ) * 8U;
        }

        

        protected abstract void DeserializeMessage(byte[] buffer, uint offsetBits);

        public DateTime UtcTime { get; set; }
        /// <summary>
        /// Time-Of-Week : Time-tag, expressed in whole milliseconds from the beginning of the current GPS week.
        /// </summary>
        public uint TOW { get; set; }
        /// <summary>
        /// The GPS week number associated with the TOW. WNc is a continuous week count(hence the "c").
        /// It is not affected by GPS week rollovers, which occur every 1024 weeks.
        /// By definition of the Galileo system time, WNc is also the Galileo week number plus 1024.
        /// </summary>
        public ushort WNc { get; set; }
    }
}