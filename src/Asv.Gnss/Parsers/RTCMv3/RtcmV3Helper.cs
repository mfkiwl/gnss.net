using System;

namespace Asv.Gnss
{
    public static class RtcmV3Helper
    {
        /// <summary>
        /// rtcm ver.3 unit of gps pseudorange (m)
        /// </summary>
        public const double PRUNIT_GPS = 299792.458;
        /// <summary>
        /// rtcm 3 unit of glo pseudorange (m)
        /// </summary>
        public const double PRUNIT_GLO = 599584.916;
        /// <summary>
        /// speed of light (m/s)
        /// </summary>
        public const double CLIGHT = 299792458.0;
        /// <summary>
        /// semi-circle to radian (IS-GPS)
        /// </summary>
        public const double SC2RAD = 3.1415926535898;
        /// <summary>
        /// L1/E1  frequency (Hz)
        /// </summary>
        public const double FREQ1 = 1.57542E9;
        /// <summary>
        ///  L2     frequency (Hz) 
        /// </summary>
        public const double FREQ2 = 1.22760E9;
        /// <summary>
        /// range in 1 ms
        /// </summary>
        public const double RANGE_MS = CLIGHT * 0.001;
        /// <summary>
        ///  2^-5
        /// </summary>
        public const double P2_5 = 0.03125;
        /// <summary>
        /// 2^-6
        /// </summary>
        public const double P2_6 = 0.015625;
        /// <summary>
        /// 2^-10
        /// </summary>
        public const double P2_10 = 0.0009765625;
        /// <summary>
        /// 2^-11
        /// </summary>
        public const double P2_11 = 4.882812500000000E-04;
        /// <summary>
        /// 2^-15
        /// </summary>
        public const double P2_15 = 3.051757812500000E-05;
        /// <summary>
        /// 2^-17
        /// </summary>
        public const double P2_17 = 7.629394531250000E-06;
        /// <summary>
        /// 2^-19
        /// </summary>
        public const double P2_19 = 1.907348632812500E-06;
        /// <summary>
        /// 2^-20
        /// </summary>
        public const double P2_20 = 9.536743164062500E-07;
        /// <summary>
        /// 2^-21
        /// </summary>
        public const double P2_21 = 4.768371582031250E-07;
        /// <summary>
        /// 2^-23
        /// </summary>
        public const double P2_23 = 1.192092895507810E-07;
        /// <summary>
        /// 2^-24
        /// </summary>
        public const double P2_24 = 5.960464477539063E-08;
        /// <summary>
        /// 2^-27
        /// </summary>
        public const double P2_27 = 7.450580596923828E-09;
        /// <summary>
        /// 2^-29
        /// </summary>
        public const double P2_29 = 1.862645149230957E-09;
        /// <summary>
        /// 2^-30
        /// </summary>
        public const double P2_30 = 9.313225746154785E-10;
        /// <summary>
        /// 2^-31
        /// </summary>
        public const double P2_31 = 4.656612873077393E-10;
        /// <summary>
        /// 2^-32
        /// </summary>
        public const double P2_32 = 2.328306436538696E-10;
        /// <summary>
        /// 2^-33
        /// </summary>
        public const double P2_33 = 1.164153218269348E-10;
        /// <summary>
        /// 2^-35
        /// </summary>
        public const double P2_35 = 2.910383045673370E-11;
        /// <summary>
        /// 2^-38
        /// </summary>
        public const double P2_38 = 3.637978807091710E-12;
        /// <summary>
        /// 2^-39
        /// </summary>
        public const double P2_39 = 1.818989403545856E-12;
        /// <summary>
        /// 2^-40
        /// </summary>
        public const double P2_40 = 9.094947017729280E-13;
        /// <summary>
        /// 2^-43
        /// </summary>
        public const double P2_43 = 1.136868377216160E-13;
        /// <summary>
        /// 2^-48
        /// </summary>
        public const double P2_48 = 3.552713678800501E-15;
        /// <summary>
        /// 2^-50
        /// </summary>
        public const double P2_50 = 8.881784197001252E-16;
        /// <summary>
        /// earth semimajor axis (WGS84) (m)
        /// </summary>
        public const double RE_WGS84 = 6378137.0;
        /// <summary>
        /// earth flattening (WGS84)
        /// </summary>
        public const double FE_WGS84 = (1.0 / 298.257223563);

        /// <summary>
        /// deg to rad
        /// </summary>
        public const double D2R = (Math.PI / 180.0);
        /// <summary>
        /// rad to deg 
        /// </summary>
        public const double R2D = (180.0 / Math.PI);


        public static DateTime GetFromGps(int weeknumber, double seconds)
        {
            var datum = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);
            var week = datum.AddDays(weeknumber * 7);
            var time = week.AddSeconds(seconds);
            return time;
        }

        public static int LeapSecondsGPS(int year, int month)
        {
            return LeapSecondsTAI(year, month) - 19;
        }

        public static int LeapSecondsTAI(int year, int month)
        {
            //http://maia.usno.navy.mil/ser7/tai-utc.dat

            var yyyymm = year * 100 + month;
            if (yyyymm >= 201701) return 37;
            if (yyyymm >= 201507) return 36;
            if (yyyymm >= 201207) return 35;
            if (yyyymm >= 200901) return 34;
            if (yyyymm >= 200601) return 33;
            if (yyyymm >= 199901) return 32;
            if (yyyymm >= 199707) return 31;
            if (yyyymm >= 199601) return 30;

            return 0;
        }

        public static void GetFromTime(DateTime time, ref int week, ref double seconds)
        {
            var datum = new DateTime(1980, 1, 6, 0, 0, 0, DateTimeKind.Utc);

            var dif = time - datum;

            var weeks = (int)(dif.TotalDays / 7);

            week = weeks;

            dif = time - datum.AddDays(weeks * 7);

            seconds = dif.TotalSeconds;
        }
    }
}