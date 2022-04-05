using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Asv.Gnss
{
    public abstract class UbxMessageBase : GnssMessageBaseWithId<ushort>
    {
        public override string ProtocolId => UbxBinaryParser.GnssProtocolId;

        public abstract byte Class { get; }
        public abstract byte SubClass { get; }
        public override ushort MessageId => (ushort)((Class << 8) | SubClass);

        public override int GetMaxByteSize()
        {
            return UbxHelper.HeaderOffset + 2;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits;

            buffer[bitIndex++] = UbxHelper.SyncByte1;
            buffer[bitIndex++] = UbxHelper.SyncByte2;
            buffer[bitIndex++] = Class;
            buffer[bitIndex++] = SubClass;
            PayloadLength = (ushort)InternalSerialize(buffer, bitIndex + 2);
            var length = BitConverter.GetBytes(PayloadLength);
            buffer[bitIndex++] = length[0];
            buffer[bitIndex++] = length[1];
            bitIndex += PayloadLength;
            var crc = UbxCrc16.CalculateCheckSum(buffer, UbxHelper.HeaderOffset + PayloadLength);
            buffer[bitIndex++] = crc.Crc1;
            buffer[bitIndex++] = crc.Crc2;

            return bitIndex - offsetBits;
        }

        protected virtual uint InternalSerialize(byte[] buffer, uint offset)
        {
            return 0;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits;
            if (buffer[0] != UbxHelper.SyncByte1 || buffer[1] != UbxHelper.SyncByte2)
            {
                throw new Exception($"Deserialization UBX message failed: want {UbxHelper.SyncByte1:X} {UbxHelper.SyncByte2:X}. Read {buffer[0]:X} {buffer[1]:X}");
            }

            var msgId = UbxHelper.ReadMessageId(buffer);
            if (msgId != MessageId)
            {
                throw new Exception($"Deserialization UBX message failed: want message number '{UbxHelper.GetMessageName(MessageId)}'. Read = '{UbxHelper.GetMessageName(msgId)}'");
            }

            var payloadLength = UbxHelper.ReadMessageLength(buffer);
            if (payloadLength > (buffer.Length - UbxHelper.HeaderOffset - 2 /* crc 16 bit*/))
            {
                throw new Exception($"Deserialization Ubx message failed: length too small. Want '{payloadLength}'. Read = '{buffer.Length - UbxHelper.HeaderOffset - 2}'");
            }

            PayloadLength = payloadLength;
            
            bitIndex += UbxHelper.HeaderOffset;

            return bitIndex - offsetBits;
        }

        public ushort PayloadLength { get; protected set; }

        public virtual byte[] GenerateRequest()
        {
            return UbxHelper.GenerateRequest(Class, SubClass);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new StringEnumConverter());
        }
    }
}