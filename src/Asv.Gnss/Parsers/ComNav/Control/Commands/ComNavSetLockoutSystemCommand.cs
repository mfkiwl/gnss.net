using System;

namespace Asv.Gnss.Control
{

    public enum ComNavSatelliteSystemEnum
    {
        GPS = 0,
        GLONASS = 1,
        SBAS = 2,
        Galileo = 5,
        BeiDou = 6,
        QZSS = 7,
        NavIC = 9,
    }
    /// <summary>
    /// Prevents the receiver from using a system
    /// This command is used to prevent the receiver from using all satellites in a system in the solution computations.
    /// </summary>
    public class ComNavSetLockoutSystemCommand:ComNavAsciiCommandBase
    {
        public ComNavSatelliteSystemEnum SatelliteSystem { get; set; }

        protected override string SerializeToAsciiString()
        {
            switch (SatelliteSystem)
            {
                case ComNavSatelliteSystemEnum.GPS:
                    return "LOCKOUTSYSTEM GPS";
                case ComNavSatelliteSystemEnum.GLONASS:
                    return "LOCKOUTSYSTEM GLONASS";
                case ComNavSatelliteSystemEnum.SBAS:
                    return "LOCKOUTSYSTEM SBAS";
                case ComNavSatelliteSystemEnum.Galileo:
                    return "LOCKOUTSYSTEM Galileo";
                case ComNavSatelliteSystemEnum.BeiDou:
                    return "LOCKOUTSYSTEM BeiDou";
                case ComNavSatelliteSystemEnum.QZSS:
                    return "LOCKOUTSYSTEM QZSS";
                case ComNavSatelliteSystemEnum.NavIC:
                    return "LOCKOUTSYSTEM NavIC";
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }

    // <summary>
}