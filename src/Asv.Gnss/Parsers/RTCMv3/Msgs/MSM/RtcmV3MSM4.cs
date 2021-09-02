using System;
using System.Linq;
using System.Text;

namespace Asv.Gnss
{
    public class RtcmV3MSM4 : RtcmV3MultipleSignalMessagesBase
    {
        private const double SLight = 299792458.0; /* speed of light (m/s) */
        private const double Freq1 = 1.57542E9; /* L1/E1  frequency (Hz) */
        private const double Freq2 = 1.22760E9; /* L2     frequency (Hz) */
        private const double RangeMs = SLight * 0.001; /* range in 1 ms */
        private const double P2_10 = 0.0009765625; /* 2^-10 */
        private const double P2_24 = 5.960464477539063E-08; /* 2^-24 */
        private const double P2_29 = 1.862645149230957E-09; /* 2^-29 */
        private const double SnrUnit = 0.001; /* SNR unit (dBHz) */

        public RtcmV3MSM4(ushort messageId)
        {
            if (messageId != 1074 && messageId != 1084 && messageId != 1094 && messageId != 1124)
            {
                throw new Exception($"Incorrect message Id {messageId}");
            }

            MessageId = messageId;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);
            var nCell = CellMask.Count(_ => _ > 0);

            var r = new double[Satellites.Length];
            var pr = new double[nCell];
            var cp = new double[nCell];
            var cnr = new double[nCell];

            var @lock = new uint[nCell];
            var half = new uint[nCell];

            for (var i = 0; i < Satellites.Length; i++) r[i] = 0.0;
            for (var i = 0; i < nCell; i++) pr[i] = cp[i] = -1E16;

            /* decode satellite data */
            for (var j = 0; j < Satellites.Length; j++)
            {
                /* range */
                var rng = RtcmV3Helper.GetBitU(buffer, bitIndex, 8);
                bitIndex += 8;
                if (rng != 255) r[j] = rng * RangeMs;
            }

            for (var j = 0; j < Satellites.Length; j++)
            {
                var rngM = RtcmV3Helper.GetBitU(buffer, bitIndex, 10);
                bitIndex += 10;
                if (r[j] != 0.0) r[j] += rngM * P2_10 * RangeMs;
            }

            /* decode signal data */
            for (var j = 0; j < nCell; j++)
            {
                /* pseudorange */
                var prv = RtcmV3Helper.GetBits(buffer, bitIndex, 15);
                bitIndex += 15;
                if (prv != -16384) pr[j] = prv * P2_24 * RangeMs;
            }

            for (var j = 0; j < nCell; j++)
            {
                /* phase range */
                var cpv = RtcmV3Helper.GetBits(buffer, bitIndex, 22);
                bitIndex += 22;
                if (cpv != -2097152) cp[j] = cpv * P2_29 * RangeMs;
            }

            for (var j = 0; j < nCell; j++)
            {
                /* lock time */
                @lock[j] = RtcmV3Helper.GetBitU(buffer, bitIndex, 4);
                bitIndex += 4;
            }

            for (var j = 0; j < nCell; j++)
            {
                /* half-cycle ambiguity */
                half[j] = RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;
            }

            for (var j = 0; j < nCell; j++)
            {
                /* cnr */
                cnr[j] = RtcmV3Helper.GetBitU(buffer, bitIndex, 6) * 1.0; bitIndex += 6;
            }

            //ToDo дальше нужно создать список Satelits Observale
            
            // Console.WriteLine("---------------------------------------------------------------------");
            // Console.WriteLine($"Msg: {MessageId}\tSatellites count: {Satellites.Length}");
            for (var i = 0; i < Satellites.Length; i++)
            {
                var j = 0;
                //var sb = new StringBuilder();

                var satelliteId = Satellites[i];
                //sb.Append($"SatelliteId: {satelliteId}\t");
                
                /* pseudorange (m) */
                if (r[i] != 0.0 && pr[j] > -1E12)
                {
                    var psRange = r[i] + pr[j];
                  //  sb.Append($"Pseudorange: {psRange}m\t");
                }

                /* carrier-phase (cycle) */
                if (r[i] != 0.0 && cp[j] > -1E12)
                {
                    var carrierPhase = (r[i] + cp[j]) * Freq1 / SLight;
                    //sb.Append($"Carrier-phase: {carrierPhase}\t");
                }


                var snr = (cnr[j] / SnrUnit + 0.5);
                //sb.Append($"SNR: {snr}");

                
                //Console.WriteLine(sb.ToString());
            }
            // Console.WriteLine("---------------------------------------------------------------------");

            return bitIndex;
        }

        public override ushort MessageId { get; }

    }
}