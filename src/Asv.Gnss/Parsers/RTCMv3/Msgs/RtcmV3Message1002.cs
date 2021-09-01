using System;
using System.Collections.Generic;

namespace Asv.Gnss
{
    public class RtcmV3Raw
    {
        public byte Amb { get; set; }
        public byte Cnr1 { get; set; }
        public byte Cnr2 { get; set; }
        public byte Code1 { get; set; }
        public byte Code2 { get; }
        public byte Lock1 { get; set; }
        public byte Lock2 { get; }
        public int Ppr1 { get; set; }
        public int Ppr2 { get; }
        public uint Pr1 { get; set; }
        public int Pr21 { get; }
        public byte Prn { get; set; }
        public byte Fcn { get; }
    }

    public class RtcmV3Observation
    {
        public RtcmV3Raw Raw = new RtcmV3Raw();

        public double Cp;
        public double Cp2;
        public double Pr;
        public double Pr2;

        public byte Prn;

        public byte Snr;
        public double Tow;
        public int Week;
        public char Sys;


    }

    /// <summary>
    /// Extended L1-Only GPS RTK Observables
    /// Расширенные измерения по частоте GPS L1
    /// </summary>
    public class RtcmV3Message1002 : RtcmV3MessageBase
    {
        private readonly int _week;

        public RtcmV3Message1002(RtcmV3Preamble preamble, RtcmV3Header header, int week) : base(preamble, header)
        {
            _week = week;
        }

        public List<RtcmV3Observation> Observations { get; } = new List<RtcmV3Observation>();

        public override void Serialize(byte[] buffer, uint startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(byte[] buffer, uint startIndex = 0)
        {
            uint i = 24 + startIndex;

            var type = RtcmV3Helper.GetBitU(buffer, i, 12);
            i += 12;

            var staid = RtcmV3Helper.GetBitU(buffer, i, 12);
            i += 12;
            var tow = RtcmV3Helper.GetBitU(buffer, i, 30) * 0.001;
            i += 30;
            var sync = RtcmV3Helper.GetBitU(buffer, i, 1);
            i += 1;
            var nsat = RtcmV3Helper.GetBitU(buffer, i, 5);

            i = 24 + 64;

            var week = _week;

            var gpstime = RtcmV3Helper.GetFromGps(week, tow);


            for (var a = 0; a < nsat; a++)
            {
                var ob = new RtcmV3Observation();
                ob.Sys = 'G';
                ob.Tow = tow;
                ob.Week = week;

                ob.Raw.Prn = (byte) RtcmV3Helper.GetBitU(buffer, i, 6);
                i += 6;
                ob.Raw.Code1 = (byte) RtcmV3Helper.GetBitU(buffer, i, 1);
                i += 1;
                ob.Raw.Pr1 = RtcmV3Helper.GetBitU(buffer, i, 24);
                i += 24;
                ob.Raw.Ppr1 = RtcmV3Helper.GetBits(buffer, i, 20);
                i += 20;
                ob.Raw.Lock1 = (byte) RtcmV3Helper.GetBitU(buffer, i, 7);
                i += 7;
                ob.Raw.Amb = (byte) RtcmV3Helper.GetBitU(buffer, i, 8);
                i += 8;
                ob.Raw.Cnr1 = (byte) RtcmV3Helper.GetBitU(buffer, i, 8);
                i += 8;

                var pr1 = ob.Raw.Pr1 * 0.02 + ob.Raw.Amb * RtcmV3Helper.PRUNIT_GPS;

                var lam1 = RtcmV3Helper.CLIGHT / RtcmV3Helper.FREQ1;

                var cp1 = ob.Raw.Ppr1 * 0.0005 / lam1;

                if ((uint) ob.Raw.Ppr1 != 0xFFF80000)
                {
                    ob.Prn = ob.Raw.Prn;
                    ob.Cp = pr1 / lam1 + cp1;
                    ob.Pr = pr1;
                    ob.Snr = (byte) (ob.Raw.Cnr1 * 0.25); // *4.0+0.5

                    Observations.Add(ob);

                }
            }
        }
    }
}