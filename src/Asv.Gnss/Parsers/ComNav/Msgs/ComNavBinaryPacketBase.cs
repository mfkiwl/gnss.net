using System;

namespace Asv.Gnss
{

    public abstract class ComNavBinaryPacketBase: GnssMessageBase, ISerializable
    {
        public override string ProtocolId => ComNavBinaryParser.GnssProtocolId;

        public abstract ushort MessageId { get; }

        public override int GetMaxByteSize()
        {
            return ComNavBinaryParser.MaxPacketSize;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var offsetInBytes = (int)(offsetBits / 8);
            var headerLength = buffer[offsetInBytes + 3];
            var msgId = BitConverter.ToUInt16(buffer, offsetInBytes + 4);
            if (msgId != MessageId) throw new GnssParserException(ComNavBinaryParser.GnssProtocolId, $"Error to deserialize BinaryComNav packet message id not equal (want [{MessageId}] read [{msgId}])");
            var messageLength = BitConverter.ToUInt16(buffer, offsetInBytes + 8); 
            GpsWeek = BitConverter.ToUInt16(buffer, offsetInBytes + 14);
            GpsTime = BitConverter.ToUInt32(buffer, offsetInBytes + 16);
            ReceiverSwVersion = BitConverter.ToUInt16(buffer, offsetInBytes + 26);
            DeserializeMessage(buffer, offsetBits + headerLength * 8U, messageLength);
            return offsetBits + headerLength * 8U + messageLength * 8U + 4U * 8U /* CRC32 */;
        }

        protected abstract void DeserializeMessage(byte[] buffer, uint offsetBits, ushort messageLength);

        /// <summary>
        /// This is a value (0 - 65535) that represents the receiver software build number
        /// </summary>
        public ushort ReceiverSwVersion { get; set; }
        /// <summary>
        /// Milliseconds from the beginning of the GPS week.
        /// </summary>
        public uint GpsTime { get; set; }
        /// <summary>
        /// GPS week number.
        /// </summary>
        public ushort GpsWeek { get; set; }
    }
}