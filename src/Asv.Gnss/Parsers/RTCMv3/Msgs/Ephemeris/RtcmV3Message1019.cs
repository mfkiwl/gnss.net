using System;

namespace Asv.Gnss
{
    public class RtcmV3Message1019 : RtcmV3MessageBase
    {
        public const int RtcmMessageId = 1019;

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var utc = DateTime.UtcNow;
            var sys = NavigationSystemEnum.SYS_GPS;
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            
            var prn = RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex += 6;
            var week = RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;
            SvAccuracy = (int)RtcmV3Helper.GetBitU(buffer, bitIndex, 4); bitIndex += 4;
            CodeL2 = (int)RtcmV3Helper.GetBitU(buffer, bitIndex, 2); bitIndex += 2;
            Idot = RtcmV3Helper.GetBits(buffer, bitIndex, 14) * RtcmV3Helper.P2_43 * RtcmV3Helper.SC2RAD; bitIndex += 14;
            Iode = (int)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            var toc = RtcmV3Helper.GetBitU(buffer, bitIndex, 16) * 16.0; bitIndex += 16;
            Af2 = RtcmV3Helper.GetBits(buffer, bitIndex, 8) * RtcmV3Helper.P2_55; bitIndex += 8;
            Af1 = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_43; bitIndex += 16;
            Af0 = RtcmV3Helper.GetBits(buffer, bitIndex, 22) * RtcmV3Helper.P2_31; bitIndex += 22;
            Iodc = (int)RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;
            Crs = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_5; bitIndex += 16;
            Deln = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_43 * RtcmV3Helper.SC2RAD; bitIndex += 16;
            M0 = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Cuc = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            E = RtcmV3Helper.GetBitU(buffer, bitIndex, 32) * RtcmV3Helper.P2_33; bitIndex += 32;
            Cus = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            var sqrtA = RtcmV3Helper.GetBitU(buffer, bitIndex, 32) * RtcmV3Helper.P2_19; bitIndex += 32;
            Toes = RtcmV3Helper.GetBitU(buffer, bitIndex, 16) * 16.0; bitIndex += 16;
            Cic = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            Omg0 = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Cis = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_29; bitIndex += 16;
            I0 = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            Crc = RtcmV3Helper.GetBits(buffer, bitIndex, 16) * RtcmV3Helper.P2_5; bitIndex += 16;
            Omg = RtcmV3Helper.GetBits(buffer, bitIndex, 32) * RtcmV3Helper.P2_31 * RtcmV3Helper.SC2RAD; bitIndex += 32;
            OmgD = RtcmV3Helper.GetBits(buffer, bitIndex, 24) * RtcmV3Helper.P2_43 * RtcmV3Helper.SC2RAD; bitIndex += 24;
            Tgd[0] = RtcmV3Helper.GetBits(buffer, bitIndex, 8) * RtcmV3Helper.P2_31; bitIndex += 8;
            SvHealth = (int)RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex += 6;
            FlagL2PData = (int)RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;
            Fit = RtcmV3Helper.GetBitU(buffer, bitIndex, 1) == 1 ? 0.0 : 4.0; /* 0:4hr,1:>4hr */ bitIndex += 1;

            if (prn >= 40)
            {
                sys = NavigationSystemEnum.SYS_SBS; 
                prn += 80;
            }

            var sat = RtcmV3Helper.satno(sys, (int) prn);

            if (sat == 0)
            {
                throw new Exception($"Rtcm3 1019 satellite number error: prn={prn}");
            }

            SatelliteNumber = sat;
            Week = RtcmV3Helper.adjgpsweek(utc, (int)week);

            var time = RtcmV3Helper.Utc2Gps(utc);

            var tt = RtcmV3Helper.GetFromGps(Week, Toes) - time;
            
            if (tt.TotalSeconds < -302400.0) Week++;
            else if (tt.TotalSeconds >= 302400.0) Week--;

            Toe = RtcmV3Helper.GetFromGps(Week, Toes);
            Toc = RtcmV3Helper.GetFromGps(Week, toc);

            TTrans = time;

            A = sqrtA * sqrtA;

            SatelliteCode = RtcmV3Helper.Sat2Code(sat, (int) prn);

            return bitIndex - offsetBits;
        }

        public override ushort MessageId => RtcmMessageId;

        /// <summary>
        /// /* satellite number */
        /// </summary>
        public int SatelliteNumber { get; set; }

        public string SatelliteCode { get; set; }
        /// <summary>
        /// IODE
        /// </summary>
        public int Iode { get; set; }
        /// <summary>
        /// IODC
        /// </summary>
        public int Iodc { get; set; }
        /// <summary>
        /// /* SV accuracy (URA index) */
        /// </summary>
        public int SvAccuracy { get; set; }
        /// <summary>
        /// /* SV health (0:ok) */
        /// </summary>
        public int SvHealth { get; set; }
        /// <summary>
        /// /* GPS/QZS: gps week, GAL: galileo week */
        /// </summary>
        public int Week { get; set; }
        /// <summary>
        /// GPS/QZS: code on L2.
        /// GAL: data source defined as rinex 3.03.
        /// BDS: data source (0:unknown,1:B1I,2:B1Q,3:B2I,4:B2Q,5:B3I,6:B3Q)
        /// </summary>
        public int CodeL2 { get; set; }
        /// <summary>
        /// GPS/QZS: L2 P data flag
        /// BDS: nav type (0:unknown,1:IGSO/MEO,2:GEO)
        /// </summary>
        public int FlagL2PData { get; set; }

        /// <summary>
        /// Toe
        /// </summary>
        public DateTime Toe { get; set; }
        /// <summary>
        /// Toc
        /// </summary>
        public DateTime Toc { get; set; }
        /// <summary>
        /// T_trans
        /// </summary>
        public DateTime TTrans { get; set; }

        /* SV orbit parameters */
        public double A { get; set; }
        public double E { get; set; }
        public double I0 { get; set; }
        public double Omg0 { get; set; }
        public double Omg { get; set; }
        public double M0 { get; set; }
        public double Deln { get; set; }
        public double OmgD { get; set; }
        public double Idot { get; set; }
        public double Crc { get; set; }
        public double Crs { get; set; }
        public double Cuc { get; set; }
        public double Cus { get; set; }
        public double Cic { get; set; }
        public double Cis { get; set; }

        /// <summary>
        /// /* Toe (s) in week */
        /// </summary>
        public double Toes { get; set; }

        /// <summary>
        /// /* fit interval (h) */
        /// </summary>
        public double Fit { get; set; }

        /// <summary>
        /// SV clock parameters af0
        /// </summary>
        public double Af0 { get; set; }
        /// <summary>
        /// SV clock parameters af1
        /// </summary>
        public double Af1 { get; set; }
        /// <summary>
        /// SV clock parameters af2
        /// </summary>
        public double Af2 { get; set; }

        /// <summary>
        /// group delay parameters.
        /// GPS/QZS:tgd[0]=TGD.
        /// GAL:tgd[0]=BGD_E1E5a,tgd[1]=BGD_E1E5b.
        /// CMP:tgd[0]=TGD_B1I ,tgd[1]=TGD_B2I/B2b,tgd[2]=TGD_B1Cp.
        /// tgd[3]=TGD_B2ap,tgd[4]=ISC_B1Cd.
        /// tgd[5]=ISC_B2ad
        /// </summary>
        public double[] Tgd { get; set; } = new double[6];

        /// <summary>
        /// Adot for CNAV
        /// </summary>
        public double Adot { get; set; }
        /// <summary>
        /// ndot for CNAV
        /// </summary>
        public double Ndot { get; set; }

    }
}