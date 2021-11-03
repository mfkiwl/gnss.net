using System;

namespace Asv.Gnss
{

    public enum SbfNavSysEnum
    {
        Unknown,
        GPS,
        SBAS,
        GLONASS,
        Galileo,
        QZSS,
        BeiDou,
        IRNS,
        LEO,
    }

    public static class SbfHelper
    {
        public static string GetRinexSatteliteCode(byte svidOrPrn, out SbfNavSysEnum nav)
        {
            nav = SbfNavSysEnum.Unknown;

            if (37 >= svidOrPrn && svidOrPrn >= 1)
            {
                nav = SbfNavSysEnum.GPS;
                return $"G{svidOrPrn:00}";
            }

            if (61 >= svidOrPrn && svidOrPrn >= 38)
            {
                nav = SbfNavSysEnum.GLONASS;
                return $"R{(svidOrPrn-37):00}";
            }

            // 62: GLONASS satellite of which the slot number is not known
            if (svidOrPrn == 62)
            {
                nav = SbfNavSysEnum.GLONASS;
                return $"R??";
            }
            if (68 >= svidOrPrn && svidOrPrn >= 63)
            {
                nav = SbfNavSysEnum.GLONASS;
                return $"R{(svidOrPrn - 38):00}";
            }
            if (106 >= svidOrPrn && svidOrPrn >= 71)
            {
                nav = SbfNavSysEnum.Galileo;
                return $"E{(svidOrPrn - 70):00}";
            }
            // 107-119: L-Band (MSS) satellite. Corresponding
            // satellite name can be found in the LBandBeams block.
            if (119 >= svidOrPrn && svidOrPrn >= 107)
            {
                nav = SbfNavSysEnum.Unknown;
                return $"NA?";
            }
            
            if (140 >= svidOrPrn && svidOrPrn >= 120)
            {
                nav = SbfNavSysEnum.SBAS;
                return $"S{(svidOrPrn - 100):00}";
            }
            if (180 >= svidOrPrn && svidOrPrn >= 141)
            {
                nav = SbfNavSysEnum.BeiDou;
                return $"C{(svidOrPrn - 140):00}";
            }
            if (187 >= svidOrPrn && svidOrPrn >= 181)
            {
                nav = SbfNavSysEnum.QZSS;
                return $"J{(svidOrPrn - 180):00}";
            }
            if (197 >= svidOrPrn && svidOrPrn >= 191)
            {
                nav = SbfNavSysEnum.IRNS;
                return $"I{(svidOrPrn - 190):00}";
            }
            if (215 >= svidOrPrn && svidOrPrn >= 198)
            {
                nav = SbfNavSysEnum.SBAS;
                return $"S{(svidOrPrn - 157):00}";
            }
            if (222 >= svidOrPrn && svidOrPrn >= 216)
            {
                nav = SbfNavSysEnum.IRNS;
                return $"I{(svidOrPrn - 208):00}";
            }
            if (245 >= svidOrPrn && svidOrPrn >= 223)
            {
                nav = SbfNavSysEnum.BeiDou;
                return $"C{(svidOrPrn - 182):00}";
            }

            return null;
        }
    }

}