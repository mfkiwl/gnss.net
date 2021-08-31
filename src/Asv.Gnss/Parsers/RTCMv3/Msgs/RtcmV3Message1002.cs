using System;

namespace Asv.Gnss
{
    public class RtcmV3Raw
    {
        public byte amb;
        public byte cnr1;
        public byte cnr2;
        public byte code1;
        public byte code2;
        public byte lock1;
        public byte lock2;
        public int ppr1;
        public int ppr2;
        public uint pr1;
        public int pr21;
        public byte prn;
        public byte fcn;
    }

    public class RtcmV3Observation
    {
        public double cp;
        public double cp2;
        public double pr;
        public double pr2;

        public byte prn;
        public RtcmV3Raw raw = new RtcmV3Raw();
        public byte snr;
        public double tow;
        public int week;
        public char sys;

        
    }


    public class RtcmV3Message1002: RtcmV3MessageBase
    {
        private readonly int _week;

        public RtcmV3Message1002(RtcmV3Preamble preamble, RtcmV3Header header, int week) : base(preamble, header)
        {
            _week = week;
        }

        public override void Serialize(byte[] buffer, uint startIndex = 0)
        {
            throw new NotImplementedException();
        }

        public override void Deserialize(byte[] buffer, uint startIndex = 0)
        {
            uint i = 24 + startIndex;

            var type = BitOperationHelper.GetBitU(buffer, i, 12);
            i += 12;

            var staid = BitOperationHelper.GetBitU(buffer, i, 12);
            i += 12;
            var tow = BitOperationHelper.GetBitU(buffer, i, 30) * 0.001;
            i += 30;
            var sync = BitOperationHelper.GetBitU(buffer, i, 1);
            i += 1;
            var nsat = BitOperationHelper.GetBitU(buffer, i, 5);

            i = 24 + 64;

            var week = _week;

            var gpstime = RtcmV3Helper.GetFromGps(week, tow);

            lasttow = tow;
            for (var a = 0; a < nsat; a++)
            {
                var ob = new RtcmV3Observation();
                ob.sys = 'G';
                ob.tow = tow;
                ob.week = week;

                ob.raw.prn = (byte)BitOperationHelper.GetBitU(buffer, i, 6);
                i += 6;
                ob.raw.code1 = (byte)BitOperationHelper.GetBitU(buffer, i, 1);
                i += 1;
                ob.raw.pr1 = BitOperationHelper.GetBitU(buffer, i, 24);
                i += 24;
                ob.raw.ppr1 = getbits(buffer, i, 20);
                i += 20;
                ob.raw.lock1 = (byte)BitOperationHelper.GetBitU(buffer, i, 7);
                i += 7;
                ob.raw.amb = (byte)BitOperationHelper.GetBitU(buffer, i, 8);
                i += 8;
                ob.raw.cnr1 = (byte)BitOperationHelper.GetBitU(buffer, i, 8);
                i += 8;

                var pr1 = ob.raw.pr1 * 0.02 + ob.raw.amb * PRUNIT_GPS;

                var lam1 = CLIGHT / FREQ1;

                var cp1 = ob.raw.ppr1 * 0.0005 / lam1;

                if ((uint)ob.raw.ppr1 != 0xFFF80000)
                {
                    ob.prn = ob.raw.prn;
                    ob.cp = pr1 / lam1 + cp1;
                    ob.pr = pr1;
                    ob.snr = (byte)(ob.raw.cnr1 * 0.25); // *4.0+0.5

                    obs.Add(ob);

                    //Console.WriteLine("G{0,2} {1,13} {2,16} {3,30}", ob.prn, ob.pr.ToString("0.000"),ob.cp.ToString("0.0000"), ob.snr.ToString("0.000"));
                }
            }
    }
}