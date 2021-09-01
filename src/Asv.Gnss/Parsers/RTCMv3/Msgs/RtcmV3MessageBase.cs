using System;

namespace Asv.Gnss
{
    public abstract class RtcmV3MessageBase:ISerializable
    {
        public int GetMaxByteSize()
        {
            return 1024 /* MAX LENGTH */ + 6 /* preamble-8bit + reserved-6bit + length-10bit + crc length 3*8=24 bit */;
        }

        public virtual uint Serialize(byte[] buffer, uint offsetBits = 0)
        {
            throw new NotImplementedException();
        }

        public virtual uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits;
            var preamble = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            if (preamble != RtcmV3Helper.SyncByte)
            {
                throw new Exception($"Deserialization RTCMv3 message failed: want {RtcmV3Helper.SyncByte:X}. Read {preamble:X}");
            }
            Reserved = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex+=6;
            var messageLength = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 6;
            if (messageLength > (buffer.Length - 3 /* crc 24 bit*/))
            {
                throw new Exception($"Deserialization RTCMv3 message failed: length too small. Want '{messageLength}'. Read = '{buffer.Length}'");
            }
            var msgId = (byte) RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12;
            if (msgId != MessageId)
            {
                throw new Exception($"Deserialization RTCMv3 message failed: want message number '{MessageId}'. Read = '{msgId}'");
            }
            return bitIndex - offsetBits;
        }

        public byte Reserved { get; set; }
        public abstract ushort MessageId { get; }
    }

    /// <summary>
    /// Table 3.5-1 Contents of the Message Header, Types 1001, 1002, 1003, 1004: GPS RTK Messages
    /// </summary>
    public abstract class RtcmV3GPSRTKMessagesBase:RtcmV3MessageBase
    {
        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);
            ReferenceStationID = RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex+=12;
            GPSEpochTimeTOW = RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12;
        }
        /// <summary>
        /// The Reference Station ID is determined by the service provider. Its 
        /// primary purpose is to link all message data to their unique source. It is 
        /// useful in distinguishing between desired and undesired data in cases 
        /// where more than one service may be using the same data link 
        /// frequency. It is also useful in accommodating multiple reference 
        /// stations within a single data link transmission. 
        /// In reference network applications the Reference Station ID plays an 
        /// important role, because it is the link between the observation messages 
        /// of a specific reference station and its auxiliary information contained in 
        /// other messages for proper operation. Thus the Service Provider should 
        /// ensure that the Reference Station ID is unique within the whole 
        /// network, and that ID’s should be reassigned only when absolutely 
        /// necessary. 
        /// Service Providers may need to coordinate their Reference Station ID
        /// assignments with other Service Providers in their region in order to 
        /// avoid conflicts. This may be especially critical for equipment 
        /// accessing multiple services, depending on their services and means of 
        /// information distribution.
        /// </summary>
        public uint ReferenceStationID { get; set; }
    }


}