using System;
using Asv.Tools;

namespace Asv.Gnss
{
    public class UbxNavPvt : UbxMessageBase
    {
        public override byte Class => 0x01;
        public override byte SubClass => 0x07;

        /// <summary>
        /// Magnetic declination accuracy. Only supported in ADR 4.10 and later. deg
        /// </summary>
        public double MagneticDeclinationAccuracy { get; set; }

        /// <summary>
        /// Magnetic declination. Only supported in ADR 4.10 and later. deg
        /// </summary>
        public double MagneticDeclination { get; set; }

        /// <summary>
        /// Heading of vehicle (2-D), this is only valid when headVehValid is set, otherwise the output is set to the heading of motion
        /// </summary>
        public double HeadingOfVehicle2D { get; set; }

        /// <summary>
        /// Invalid lon, lat, height and hMSL
        /// </summary>
        public bool IsValidLLH { get; set; }

        /// <summary>
        /// Position DOP
        /// </summary>
        public double PositionDOP { get; set; }


        /// <summary>
        /// Heading accuracy estimate (both motion and vehicle), deg
        /// </summary>
        public double HeadingAccuracyEstimate { get; set; }


        /// <summary>
        /// Speed accuracy estimate, m\s
        /// </summary>
        public double SpeedAccuracyEstimate { get; set; }

        /// <summary>
        /// Heading of motion (2-D), deg
        /// </summary>
        public double HeadingOfMotion2D { get; set; }


        /// <summary>
        /// Ground Speed (2-D), m\s
        /// </summary>
        public double GroundSpeed2D { get; set; }

        /// <summary>
        /// NED down velocity, m\s
        /// </summary>
        public double VelocityDown { get; set; }

        /// <summary>
        /// NED east velocity, m\s
        /// </summary>
        public double VelocityEast { get; set; }

        /// <summary>
        /// NED north velocity, m\s
        /// </summary>
        public double VelocityNorth { get; set; }

        /// <summary>
        /// Vertical accuracy estimate, m
        /// </summary>
        public double VerticalAccuracyEstimate { get; set; }

        /// <summary>
        /// Horizontal accuracy estimate, m
        /// </summary>
        public double HorizontalAccuracyEstimate { get; set; }

        /// <summary>
        /// Height above mean sea level, m
        /// </summary>
        public double AltMsl { get; set; }

        /// <summary>
        /// Height above ellipsoid, m
        /// </summary>
        public double AltElipsoid { get; set; }

        /// <summary>
        /// Latitude
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Longitude
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Number of satellites used in Nav Solution
        /// </summary>
        public byte NumberOfSatellites { get; set; }


        /// <summary>
        /// UTC Time of Day could be confirmed (see Time Validity section for details)
        /// </summary>
        public bool UTCConfirmedTime { get; set; }

        /// <summary>
        /// UTC Date validity could be confirmed (see Time Validity section for details)
        /// </summary>
        public bool UTCConfirmedDate { get; set; }

        /// <summary>
        /// 1 = information about UTC Date and Time of Day validity confirmation is available (see Time Validity section for details)
        /// This flag is only supported in Protocol Versions 19.00, 19.10, 20.10, 20.20, 20.30, 22.00, 23.00, 23.01, 27 and 28
        /// </summary>
        public bool UTCConfirmedAvailable { get; set; }

        /// <summary>
        /// ???
        /// </summary>
        public byte PsmState { get; set; }

        public enum CarrierSolutionStatus
        {
            /// <summary>
            /// no carrier phase range solution
            /// </summary>
            NoCarrierSolution = 0,
            /// <summary>
            /// carrier phase range solution with floating ambiguities
            /// </summary>
            FloatingAmbiguities = 1,
            /// <summary>
            /// carrier phase range solution with fixed ambiguities
            /// </summary>
            FixedAmbiguities = 2,
        }
        public CarrierSolutionStatus CarrierSolution { get; set; }

        /// <summary>
        /// valid fix (i.e within DOP & accuracy masks)
        /// </summary>
        public bool GnssFixOK { get; set; }

        /// <summary>
        /// Differential corrections were applied
        /// </summary>
        public bool IsAppliedDifferentialCorrections { get; set; }

        /// <summary>
        /// Differential corrections were applied
        /// </summary>
        public bool IsValidVehicleHeading { get; set; }

        public enum GNSSFixType : byte
        {
            NoFix = 0,
            DeadReckoningOnly = 1,
            Fix2D = 2,
            Fix3D = 3,
            GNSSDeadReckoning = 4,
            TimeOnlyFix = 5,
        }

        /// <summary>
        /// GNSSfix Type
        /// </summary>
        public GNSSFixType FixType { get; set; }

        /// <summary>
        /// Fraction of second, range -1e9 .. 1e9 (UTC)
        /// </summary>
        public double UTCFractionOfSecond { get; set; }

        /// <summary>
        /// Time accuracy estimate (UTC)
        /// </summary>
        public double UTCTimeAccuracyEstimate { get; set; }

        /// <summary>
        /// valid magnetic declination
        /// </summary>
        public bool IsValidMagneticDeclination { get; set; }

        /// <summary>
        /// = UTC time of day has been fully resolved (no seconds uncertainty). Cannot be used to check if time is completely solved.
        /// </summary>
        public bool UTCTimeOfDayIsFullyResolved { get; set; }

        /// <summary>
        /// valid UTC time of day (see Time Validity section for details)
        /// </summary>
        public bool UTCTimeIsConfirmation { get; set; }

        /// <summary>
        /// valid UTC Date (see Time Validity section for details)
        /// </summary>
        public bool UTCDateIsConfirmation { get; set; }

        /// <summary>
        /// Seconds of minute, range 0..60 (UTC)
        /// </summary>
        public byte Sec { get; set; }

        /// <summary>
        /// Minute of hour, range 0..59 (UTC)
        /// </summary>
        public byte Min { get; set; }

        /// <summary>
        /// Hour of day, range 0..23 (UTC)
        /// </summary>
        public byte Hour { get; set; }

        /// <summary>
        /// Day of month, range 1..31 (UTC)
        /// </summary>
        public byte Day { get; set; }

        /// <summary>
        /// Month, range 1..12 (UTC)
        /// </summary>
        public byte Month { get; set; }

        /// <summary>
        /// Year (UTC)
        /// </summary>
        public ushort Year { get; set; }

        /// <summary>
        /// GPS time of week of the navigation epoch.
        /// See the description of iTOW for details.
        /// </summary>
        public ulong iTOW { get; set; }

        public GeoPoint MovingBaseLocation { get; set; }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            iTOW = BitConverter.ToUInt32(buffer, (int)byteIndex); byteIndex += 4;
            Year = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            Month = buffer[byteIndex++];
            Day = buffer[byteIndex++];
            Hour = buffer[byteIndex++];
            Min = buffer[byteIndex++];
            Sec = buffer[byteIndex++];

            //UTC Date and Time Confirmation Status   Date: CONFIRMED, Time: CONFIRMED

            UTCDateIsConfirmation = (buffer[byteIndex] & 0b0000_0001) != 0;
            UTCTimeIsConfirmation = (buffer[byteIndex] & 0b0000_0010) != 0;
            UTCTimeOfDayIsFullyResolved = (buffer[byteIndex] & 0b0000_0100) != 0;
            IsValidMagneticDeclination = (buffer[byteIndex++] & 0b0000_1000) != 0;

            UTCTimeAccuracyEstimate = BitConverter.ToUInt32(buffer, (int)byteIndex) * 1e-9; byteIndex += 4;
            UTCFractionOfSecond = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            FixType = (GNSSFixType)buffer[byteIndex++];

            GnssFixOK = (buffer[byteIndex] & 0b0000_0001) != 0;
            IsAppliedDifferentialCorrections = (buffer[byteIndex] & 0b0000_0010) != 0;
            IsValidVehicleHeading = (buffer[byteIndex] & 0b0010_0000) != 0;
            PsmState = (byte)((buffer[byteIndex] & 0b0001_1100) >> 2);
            CarrierSolution = (CarrierSolutionStatus)((buffer[byteIndex++] & 0b1100_0000) >> 6);

            UTCConfirmedAvailable = (buffer[byteIndex] & 0b0010_0000) != 0;
            UTCConfirmedDate = (buffer[byteIndex] & 0b0100_0000) != 0;
            UTCConfirmedTime = (buffer[byteIndex++] & 0b1000_0000) != 0;

            NumberOfSatellites = buffer[byteIndex++];
            Longitude = BitConverter.ToInt32(buffer, (int)byteIndex) * 1e-7; byteIndex += 4;
            Latitude = BitConverter.ToInt32(buffer, (int)byteIndex) * 1e-7; byteIndex += 4;
            AltElipsoid = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            AltMsl = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            HorizontalAccuracyEstimate = BitConverter.ToUInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            VerticalAccuracyEstimate = BitConverter.ToUInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            VelocityNorth = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            VelocityEast = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            VelocityDown = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            GroundSpeed2D = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            HeadingOfMotion2D = BitConverter.ToInt32(buffer, (int)byteIndex) * 1e-5; byteIndex += 4;
            SpeedAccuracyEstimate = BitConverter.ToUInt32(buffer, (int)byteIndex) * 0.001; byteIndex += 4;
            HeadingAccuracyEstimate = BitConverter.ToUInt32(buffer, (int)byteIndex) * 1e-5; byteIndex += 4;
            PositionDOP = BitConverter.ToUInt16(buffer, (int)byteIndex) * 0.01; byteIndex += 2;
            IsValidLLH = (buffer[byteIndex] & 0b0000_0001) == 0; byteIndex += 6;
            HeadingOfVehicle2D = BitConverter.ToInt32(buffer, (int)byteIndex) * 1e-5; byteIndex += 4;
            MagneticDeclination = BitConverter.ToInt16(buffer, (int)byteIndex) * 1e-2; byteIndex += 2;
            MagneticDeclinationAccuracy = BitConverter.ToUInt16(buffer, (int)byteIndex) * 1e-2; byteIndex += 2;

            if (FixType >= GNSSFixType.Fix3D && GnssFixOK)
            {
                MovingBaseLocation = new GeoPoint(Latitude, Longitude, AltElipsoid);
            }

            return byteIndex * 8 - offsetBits;
        }
    }
}