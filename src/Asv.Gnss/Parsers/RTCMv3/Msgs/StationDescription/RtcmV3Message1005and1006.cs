namespace Asv.Gnss
{
    public abstract class RtcmV3Message1005and1006 : RtcmV3MessageBase
    {
        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            var rr = new double[3];
            var re = new double[3];
            var pos = new double[3];
            
            ReferenceStationID = RtcmV3Helper.GetBitU(buffer, bitIndex, 12); bitIndex += 12;
            
            ITRF = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex += 6 + 4;
            
            rr[0] = RtcmV3Helper.GetBits38(buffer, bitIndex); bitIndex += 38 + 2;
            rr[1] = RtcmV3Helper.GetBits38(buffer, bitIndex); bitIndex += 38 + 2;
            rr[2] = RtcmV3Helper.GetBits38(buffer, bitIndex); bitIndex += 38;

            for (var i = 0; i < 3; i++) 
                re[i] = rr[i] * 0.0001;

            RtcmV3Helper.EcefToPos(re, pos);

            X = rr[0] * 0.0001;
            Y = rr[1] * 0.0001;
            Z = rr[2] * 0.0001;

            Latitude = pos[0] * RtcmV3Helper.R2D;
            Longitude = pos[1] * RtcmV3Helper.R2D;
            Altitude = pos[2];

            Height = 0.0;

            return bitIndex - offsetBits;
        }

        /// <summary>
        /// Antenna Height
        /// </summary>
        public double Height { get; set; }
        /// <summary>
        /// Antenna Reference Point ECEF-X 
        /// </summary>
        public double X { get; set; }
        /// <summary>
        /// Antenna Reference Point ECEF-Y 
        /// </summary>
        public double Y { get; set; }
        /// <summary>
        /// Antenna Reference Point ECEF-Z 
        /// </summary>
        public double Z { get; set; }

        /// <summary>
        /// Antenna Reference Point WGS84-Latitude
        /// </summary>
        public double Latitude { get; set; }
        /// <summary>
        /// Antenna Reference Point WGS84-Longitude
        /// </summary>
        public double Longitude { get; set; }
        /// <summary>
        /// Antenna Reference Point WGS84-Altitude
        /// </summary>
        public double Altitude { get; set; }

        /// <summary>
        /// Since this field is reserved, all bits should be set to zero for now.
        /// However, since the value is subject to change in future versions,
        /// decoding should not rely on a zero value.
        /// The ITRF realization year identifies the datum definition used for
        /// coordinates in the message. 
        /// </summary>
        public byte ITRF { get; set; }

        
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

    public class RtcmV3Message1005 : RtcmV3Message1005and1006
    {
        public const int RtcmMessageId = 1005;

        public override ushort MessageId => RtcmMessageId;
    }

    public class RtcmV3Message1006 : RtcmV3Message1005and1006
    {
        public const int RtcmMessageId = 1006;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            Height = RtcmV3Helper.GetBitU(buffer, bitIndex, 16) * 0.0001; bitIndex += 16;

            return bitIndex - offsetBits;
        }

        public override ushort MessageId => RtcmMessageId;
    }
}