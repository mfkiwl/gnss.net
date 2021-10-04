using System;
using System.Text;

namespace Asv.Gnss
{

   

    /// <summary>
    /// 4.2.7.5 PSRPOS Pseudorange Position
    /// This message includes position calculated using pseudorange and other information such as differential age, station id and so on.
    /// </summary>
    public class ComNavBinaryPsrPosPacket : ComNavBinaryPacketBase
    {
        public const int ComNavMessageId = 47;

        public override ushort MessageId => ComNavMessageId;

        protected override void DeserializeMessage(byte[] buffer, uint offsetBits, ushort messageLength)
        {
            var offsetInBytes = (int) (offsetBits / 8);
            SolutionStatus = ComNavBinaryHelper.ParseSolutionStatus(buffer, offsetInBytes);
            PositionType = ComNavBinaryHelper.ParsePositionType(buffer, offsetInBytes + 4);

            Latitude = BitConverter.ToDouble(buffer, offsetInBytes + 8);
            Longitude = BitConverter.ToDouble(buffer, offsetInBytes + 16);
            HeightMsl = BitConverter.ToDouble(buffer, offsetInBytes + 24);
            Undulation = BitConverter.ToSingle(buffer, offsetInBytes + 32);

            Datum = ComNavBinaryHelper.ParseDatum(buffer, offsetInBytes + 36);

            LatitudeSd = BitConverter.ToSingle(buffer, offsetInBytes + 40);
            LongitudeSd = BitConverter.ToSingle(buffer, offsetInBytes + 44);
            HeightMslSd = BitConverter.ToSingle(buffer, offsetInBytes + 48);

            BaseStationId = Encoding.ASCII.GetString(buffer, offsetInBytes + 52, 4);

            DifferentialAgeSec = BitConverter.ToSingle(buffer, offsetInBytes + 56);
            SolutionAgeSec = BitConverter.ToSingle(buffer, offsetInBytes + 60);

            TracketSats = buffer[offsetInBytes + 64];

            SolutionSats = buffer[offsetInBytes + 65];

            ExtSolutionStatus = buffer[offsetInBytes + 69];

            SignalMask = buffer[offsetInBytes + 71];
        }
        /// <summary>
        /// Signals used mask - if 0, signals used in solution are unknown. See Table 33.
        /// </summary>
        public byte SignalMask { get; set; }

        /// <summary>
        /// Extended solution status (default: 0)
        /// </summary>
        public byte ExtSolutionStatus { get; set; }

        /// <summary>
        /// Number of satellite vehicles used in solution
        /// </summary>
        public byte SolutionSats { get; set; }

        /// <summary>
        /// Number of satellite vehicles tracked
        /// </summary>
        public byte TracketSats { get; set; }


        /// <summary>
        /// Solution age in seconds
        /// </summary>
        public float SolutionAgeSec { get; set; }

        /// <summary>
        /// Differential age in seconds
        /// </summary>
        public float DifferentialAgeSec { get; set; }

        /// <summary>
        /// This is station ID of the station, who sending DGPS corrections (Not Current station id!)
        /// </summary>
        public string BaseStationId { get; set; }

        /// <summary>
        /// Height standard deviation
        /// </summary>
        public float HeightMslSd { get; set; }
        /// <summary>
        /// Longitude standard deviation
        /// </summary>
        public float LongitudeSd { get; set; }
        /// <summary>
        /// Latitude standard deviation
        /// </summary>
        public float LatitudeSd { get; set; }


        public ComNavDatum Datum { get; set; }
        /// <summary>
        /// Undulation - the relationship between the geoids and the WGS84 ellipsoid (m)
        /// </summary>
        public float Undulation { get; set; }

        /// <summary>
        /// Height above mean sea level
        /// </summary>
        public double HeightMsl { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public ComNavPositionType PositionType { get; set; }

        public ComNavSolutionStatus SolutionStatus { get; set; }
        
    }
}