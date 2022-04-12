using System;
using Newtonsoft.Json;

namespace Asv.Gnss
{
    public abstract class GnssMessageBase : ISerializable
    {
        /// <summary>
        /// This is for custom use (like routing, etc...)
        /// </summary>
        public object Tag { get; set; }

        public abstract string ProtocolId { get; }
        public abstract int GetMaxByteSize();
        public abstract uint Serialize(byte[] buffer, uint offsetBits);
        public abstract uint Deserialize(byte[] buffer, uint offsetBits);

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public abstract class GnssRawPacket<TMsgId>
    {
        public abstract string ProtocolId { get; }
        public TMsgId MessageId { get; }
        public byte[] RawData { get; private set; }

        protected GnssRawPacket(TMsgId messageId)
        {
            MessageId = messageId;
        }

        public uint Deserialize(byte[] buffer, int offsetByte, int lengthByte)
        {
            var message = new byte[lengthByte];
            Array.Copy(buffer, offsetByte, message, 0, lengthByte);
            RawData = message;
            return (uint)(offsetByte + lengthByte);
        }
    }

    public abstract class GnssMessageBaseWithId<TMsgId> : GnssMessageBase
    {
        public abstract TMsgId MessageId { get; }
    }
}