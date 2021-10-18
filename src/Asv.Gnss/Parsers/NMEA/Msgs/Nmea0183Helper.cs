using System;
using System.Globalization;

namespace Asv.Gnss
{
    public static class Nmea0183Helper
    {
        public static string TryFindSourceTitleById(string value)
        {
            switch (value)
            {
                case "AG":
                    return "Autopilot - General";
                case "AP":
                    return "Autopilot - Magnetic";

                case "CD":
                    return "Communications – Digital Selective Calling (DSC)";

                case "CR":
                    return "Communications – Receiver / Beacon Receiver";

                case "CS":
                    return "Communications – Satellite";

                case "CT":
                    return "Communications – Radio-Telephone (MF/HF)";

                case "CV":
                    return "Communications – Radio-Telephone (VHF)";

                case "CX":
                    return "Communications – Scanning Receiver";

                case "DF":
                    return "Direction Finder";

                case "EC":
                    return "Electronic Chart Display & Information System (ECDIS)";

                case "EP":
                    return "Emergency Position Indicating Beacon (EPIRB)";

                case "ER":
                    return "Engine Room Monitoring Systems";

                case "GP":
                    return "Global Positioning System (GPS)";

                case "HC":
                    return "Heading – Magnetic Compass";

                case "HE":
                    return "Heading – North Seeking Gyro";

                case "HN":
                    return "Heading – Non North Seeking Gyro";

                case "II":
                    return "Integrated Instrumentation";

                case "IN":
                    return "Integrated Navigation";

                case "LC":
                    return "Loran C";

                case "P ":
                    return "roprietary Code";

                case "RA":
                    return "RADAR and/or ARPA";

                case "SD":
                    return "Sounder, Depth";

                case "SN":
                    return "Electronic Positioning System, other/general";

                case "SS":
                    return "Sounder, Scanning";

                case "TI":
                    return "Turn Rate Indicator";

                case "VD":
                    return "Velocity Sensor, Doppler, other/general";

                case "DM":
                    return "Velocity Sensor, Speed Log, Water, Magnetic";

                case "VW":
                    return "Velocity Sensor, Speed Log, Water, Mechanical";

                case "WI":
                    return "Weather Instruments";

                case "YX":
                    return "Transducer";

                case "ZA":
                    return "Timekeeper – Atomic Clock";

                case "ZC":
                    return "Timekeeper – Chronometer";

                case "ZQ":
                    return "Timekeeper – Quartz";

                case "ZV":
                    return "Timekeeper – Radio Update, WWV or WWVH";

                default:
                    return "Unknown";

            }
        }

        /// <summary>
        /// hhmmss.ss | hhmmss | 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ParseTime(string token)
        {
            var temp = double.Parse(token, CultureInfo.InvariantCulture);

            var sss = (int)((temp - (int)temp) * 1000.0);
            var hh = (int)((int)temp / 10000.0);
            var mm = (int)(((int)temp - hh * 10000.0) / 100.0);
            var ss = (int)((int)temp - hh * 10000.0 - mm * 100.0);

            var now = DateTime.Now;
            return new DateTime(now.Year, now.Month, now.Day, hh, mm, ss, sss);

            // var hh = int.Parse(value.Substring(0, 2), CultureInfo.InvariantCulture);
            // var mm = int.Parse(value.Substring(2, 2), CultureInfo.InvariantCulture);
            // var ss = double.Parse(value.Substring(2, 4), CultureInfo.InvariantCulture);
            // return new DateTime(0,0,0,hh,mm,00).AddSeconds(ss);

        }

        /// <summary>
        /// ddmmyy
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ParseDate(string token)
        {
            if (token.Length != 6)
            {
                throw new ArgumentException(string.Format("Date format incorrect in \"{0}\" (must be ddmmyy)", token));
            }
                

            var date = Convert.ToInt32(token.Substring(0, 2));
            var month = Convert.ToInt32(token.Substring(2, 2));
            var year = Convert.ToInt32(token.Substring(4, 2)) + 2000;

            return new DateTime(year, month, date);

        }

        /// <summary>
        /// dd/mm/yy
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DateTime ParseTimeWithSlashes(string token)
        {
            var splits = token.Split("/".ToCharArray());

            if (splits.Length != 3)
            {
                throw new ArgumentException(string.Format("Date format incorrect in \"{0}\" (must be dd/mm/yy)",  token));
            }
                
            var date = int.Parse(splits[0]);
            var month = int.Parse(splits[1]);
            var year = int.Parse(splits[2]) + 2000;

            return new DateTime(year, month, date);

        }
        /// <summary>
        /// llll.ll
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static double ParseLatitude(string token)
        {
            var temp = double.Parse(token, CultureInfo.InvariantCulture);

            double degree = (int)((int)temp / 100.0);
            var minutes = ((int)temp - degree * 100.0);
            var seconds = (temp - (int)temp) * 60.0;

            return degree + minutes / 60.0 + seconds / 3600.0;

            // var deg = int.Parse(token.Substring(0, 2), CultureInfo.InvariantCulture);
            // var min = double.Parse(token.Substring(2, 5), CultureInfo.InvariantCulture);
            // return deg + min / 60.0;
        }
        /// <summary>
        /// yyyyy.yy
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static double ParseLongitude(string token)
        {
            var temp = double.Parse(token, CultureInfo.InvariantCulture);

            double degree = (int)((int)temp / 100.0);
            var minutes = ((int)temp - degree * 100.0);
            var seconds = (temp - (int)temp) * 60.0;

            return degree + minutes / 60.0 + seconds / 3600.0;
            // var deg = int.Parse(token.Substring(0, 3), CultureInfo.InvariantCulture);
            // var min = double.Parse(token.Substring(2, 5), CultureInfo.InvariantCulture);
            // return deg + min / 60.0;
        }

        /// <summary>
        /// dddmm.mmm
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static double ParseCommonDegrees(string token)
        {
            if (token.IsNullOrWhiteSpace()) return Double.NaN;

            var temp = double.Parse(token, CultureInfo.InvariantCulture);

            double degree = (int)((int)temp / 100.0);
            var minutes = ((int)temp - degree * 100.0);
            var seconds = (temp - (int)temp) * 60.0;

            return degree + minutes / 60.0 + seconds / 3600.0;
        }

        public static string ParseNorthSouth(string value)
        {
            return value;
        }

        public static string ParseEastWest(string value)
        {
            return value;
        }

        public static GpsQuality ParseGpsQuality(string value)
        {
            if (value.IsNullOrWhiteSpace()) return GpsQuality.Unknown;
            return (GpsQuality) int.Parse(value, CultureInfo.InvariantCulture);
        }

        public static DataStatus ParseDataStatus(string value)
        {
            if (string.Equals(value, "A", StringComparison.InvariantCultureIgnoreCase)) return DataStatus.Valid;
            if (string.Equals(value, "V", StringComparison.InvariantCultureIgnoreCase)) return DataStatus.Invalid;
            return DataStatus.Unknown;
        }

        /// <summary>
        /// x | xx | xxx | xxxx | xxxxx | xxxxxx
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int? ParseInt(string value)
        {
            if (value.IsNullOrWhiteSpace()) return null;
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// x.x
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double ParseDouble(string value)
        {
            if (value.IsNullOrWhiteSpace()) return double.NaN;
            return double.Parse(value, CultureInfo.InvariantCulture);
        }
    }

    public enum GpsQuality
    {
        Unknown = -1,
        FixNotAvailable = 0,
        GPSFix = 1,
        DifferentialGPSFix = 2,
        /// <summary>
        /// Real-Time Kinematic, fixed integers
        /// </summary>
        RTKFixed = 4,
        /// <summary>
        /// Real-Time Kinematic, float integers, OmniSTAR XP/HP or Location RTK
        /// </summary>
        RTKFloat = 5,

    }

    public enum DataStatus
    {
        Unknown,
        Valid,
        Invalid
    }
}