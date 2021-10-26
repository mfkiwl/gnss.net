using System;
using System.Threading;

namespace Asv.Gnss
{
    public abstract class RtcmV3MessageBase: GnssMessageBaseWithId<ushort>
    {
        public override string ProtocolId => RtcmV3Parser.GnssProtocolId;

        public override int GetMaxByteSize()
        {
            return 1024 /* MAX LENGTH */ + 6 /* preamble-8bit + reserved-6bit + length-10bit + crc length 3*8=24 bit */;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits = 0)
        {
            throw new NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits;
            var preamble = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            if (preamble != RtcmV3Helper.SyncByte)
            {
                throw new Exception($"Deserialization RTCMv3 message failed: want {RtcmV3Helper.SyncByte:X}. Read {preamble:X}");
            }
            Reserved = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex+=6;
            var messageLength = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;
            if (messageLength > (buffer.Length - 3 /* crc 24 bit*/))
            {
                throw new Exception($"Deserialization RTCMv3 message failed: length too small. Want '{messageLength}'. Read = '{buffer.Length}'");
            }
            var msgId = RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12;
            if (msgId != MessageId)
            {
                throw new Exception($"Deserialization RTCMv3 message failed: want message number '{MessageId}'. Read = '{msgId}'");
            }
            return bitIndex - offsetBits;
        }

        public byte Reserved { get; set; }
    }
}