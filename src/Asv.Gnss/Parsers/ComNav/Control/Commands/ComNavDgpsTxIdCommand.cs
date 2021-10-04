using System;
using System.Globalization;

namespace Asv.Gnss.Control
{

    public enum DgpsTxIdEnum
    {
        RTCM = 0,
        RTCA = 1,
        CMR = 2,
        AUTO = 10,
        RTCMV3 = 13,
        NOVATELX = 14
    }
    public class ComNavDgpsTxIdCommand : ComNavAsciiCommandBase
    {
        public DgpsTxIdEnum Type { get; set; }

        protected override string SerializeToAsciiString()
        {
            switch (Type)
            {
                case DgpsTxIdEnum.RTCM:
                    return $"DGPSTXID RTCM {Id:0000}";
                case DgpsTxIdEnum.RTCA:
                    return $"DGPSTXID RTCA {Id:0000}";
                case DgpsTxIdEnum.CMR:
                    return $"DGPSTXID CMR {Id:0000}";
                case DgpsTxIdEnum.AUTO:
                    return $"DGPSTXID AUTO {Id:0000}";
                case DgpsTxIdEnum.RTCMV3:
                    return $"DGPSTXID RTCMV3 {Id:0000}";
                case DgpsTxIdEnum.NOVATELX:
                    return $"DGPSTXID NOVATELX {Id:0000}";
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

       public uint Id { get; set; }
    }
}