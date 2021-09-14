using System;

namespace Asv.Gnss
{
    public class RtcmV3Message1013 : RtcmV3MessageBase
    {
        public const int RtcmMessageId = 1013;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            ReferenceStationID = RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12 + 16;
            // ModifiedJulianDay = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 16); bitIndex += 16;
            var secondsOfDDay = (uint)RtcmV3Helper.GetBitU(buffer, bitIndex, 17); bitIndex += 17;
            var msgCount = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 5); bitIndex += 5;
            var leapSeconds = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            var dateTime = RtcmV3Helper.GetUtc(DateTime.UtcNow, secondsOfDDay);
            EpochTime = dateTime.AddSeconds(leapSeconds);


            RtcmV3Helper.AdjustWeekly(DateTime.UtcNow, secondsOfDDay);

            SystemMessages = new SystemMessage[msgCount];

            for (var i = 0; i < msgCount; i++)
            {
                var message = new SystemMessage();
                bitIndex += message.Deserialize(buffer, bitIndex);
                SystemMessages[i] = message;
            }
            return bitIndex - offsetBits;
        }

        public DateTime EpochTime { get; set; }

        public override ushort MessageId => RtcmMessageId;

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

        public SystemMessage[] SystemMessages { get; set; }
    }

    public class SystemMessage : ISerializable
    {
        /// <summary>
        /// Each announcement lists the Message ID as transmitted by the reference station.
        /// </summary>
        public ushort Id { get; set; }
        /// <summary>
        /// 0 - Asynchronous – not transmitted on a regular basis;
        /// 1 - Synchronous – scheduled for transmission at regular intervals
        /// </summary>
        public byte SyncFlag { get; set; }

        /// <summary>
        /// Each announcement lists the Message Transmission Interval as
        /// transmitted by the reference station. If asynchronous, the transmission
        /// interval is approximate. 
        /// </summary>
        public double TransmissionInterval { get; set; }


        public int GetMaxByteSize()
        {
            return 4;
        }

        public uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits;

            Id = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12;
            SyncFlag = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;
            TransmissionInterval = RtcmV3Helper.GetBitU(buffer, bitIndex, 16) * 0.1; bitIndex += 16;
            
            return bitIndex - offsetBits;
        }
    }
}
