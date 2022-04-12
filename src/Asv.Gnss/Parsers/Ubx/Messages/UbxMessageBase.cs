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
            var byteIndex = offsetBits / 8;
            
            buffer[byteIndex++] = UbxHelper.SyncByte1;
            buffer[byteIndex++] = UbxHelper.SyncByte2;
            buffer[byteIndex++] = Class;
            buffer[byteIndex++] = SubClass;
            PayloadLength = (ushort)InternalSerialize(buffer, byteIndex + 2);
            var length = BitConverter.GetBytes(PayloadLength);
            buffer[byteIndex++] = length[0];
            buffer[byteIndex++] = length[1];
            byteIndex += PayloadLength;
            var crc = UbxCrc16.CalculateCheckSum(buffer, UbxHelper.HeaderOffset + PayloadLength);
            buffer[byteIndex++] = crc.Crc1;
            buffer[byteIndex++] = crc.Crc2;

            return byteIndex * 8 - offsetBits;
        }

        protected virtual uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            return 0;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = offsetBits / 8;
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
            
            byteIndex += UbxHelper.HeaderOffset;

            return byteIndex * 8 - offsetBits;
        }

        protected ushort PayloadLength { get; private set; }

        public virtual GnssMessageBase GetRequest()
        {
            return new UbxMessageRequest(Class, SubClass);
        }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new StringEnumConverter());
        }
    }

    public class UbxMessageRequest : UbxMessageBase
    {
        public override byte Class { get; }
        public override byte SubClass { get; }

        public UbxMessageRequest(byte @class, byte subClass)
        {
            Class = @class;
            SubClass = subClass;
        }
    }


}