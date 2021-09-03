﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Asv.Gnss
{
    public class RtcmV3MSM4 : RtcmV3MultipleSignalMessagesBase
    {
        // private const double SLight = 299792458.0; /* speed of light (m/s) */
        // private const double Freq1 = 1.57542E9; /* L1/E1  frequency (Hz) */
        // private const double Freq2 = 1.22760E9; /* L2     frequency (Hz) */
        // private const double RangeMs = SLight * 0.001; /* range in 1 ms */
        // private const double P2_10 = 0.0009765625; /* 2^-10 */
        // private const double P2_24 = 5.960464477539063E-08; /* 2^-24 */
        // private const double P2_29 = 1.862645149230957E-09; /* 2^-29 */
        // private const double SnrUnit = 0.001; /* SNR unit (dBHz) */

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
                if (rng != 255) r[j] = rng * RtcmV3Helper.RANGE_MS;
            }

            for (var j = 0; j < Satellites.Length; j++)
            {
                var rngM = RtcmV3Helper.GetBitU(buffer, bitIndex, 10);
                bitIndex += 10;
                if (r[j] != 0.0) r[j] += rngM * RtcmV3Helper.P2_10 * RtcmV3Helper.RANGE_MS;
            }

            /* decode signal data */
            for (var j = 0; j < nCell; j++)
            {
                /* pseudorange */
                var prv = RtcmV3Helper.GetBits(buffer, bitIndex, 15);
                bitIndex += 15;
                if (prv != -16384) pr[j] = prv * RtcmV3Helper.P2_24 * RtcmV3Helper.RANGE_MS;
            }

            for (var j = 0; j < nCell; j++)
            {
                /* phase range */
                var cpv = RtcmV3Helper.GetBits(buffer, bitIndex, 22);
                bitIndex += 22;
                if (cpv != -2097152) cp[j] = cpv * RtcmV3Helper.P2_29 * RtcmV3Helper.RANGE_MS;
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

            CreateMsmObservable(r, pr, cp, cnr);

            return bitIndex;
        }

        private void CreateMsmObservable(double[] r, double[] pr, double[] cp, double[] cnr)
        {
            var sig = new string[Signals.Length];
            var code = new byte[Signals.Length];
            var idx = new int[Signals.Length];
            var q = string.Empty;


            var sys = RtcmV3Helper.GetNavigationSystem(MessageId);
            switch (sys)
            {
                case NavigationSystemEnum.SYS_GPS:
                    // msm_type=q=rtcm->msmtype[0];
                    break;
                case NavigationSystemEnum.SYS_SBS:
                    // SYS_SBS: msm_type=q=rtcm->msmtype[4];
                    break;
                case NavigationSystemEnum.SYS_GLO:
                    // msm_type=q=rtcm->msmtype[1];
                    break;
                case NavigationSystemEnum.SYS_GAL:
                    // msm_type=q=rtcm->msmtype[2];
                    break;
                case NavigationSystemEnum.SYS_QZS:
                    // msm_type=q=rtcm->msmtype[3];
                    break;
                case NavigationSystemEnum.SYS_CMP:
                    // SYS_CMP: msm_type=q=rtcm->msmtype[5];
                    break;
                case NavigationSystemEnum.SYS_IRN:
                    // msm_type = q = rtcm->msmtype[6];
                    break;
                default:
                    // msm_type = q = "";
                    break;
            }

            for (var i = 0; i < Signals.Length; i++)
            {
                switch (sys)
                {
                    case NavigationSystemEnum.SYS_GPS:
                        // msm_type=q=rtcm->msmtype[0];
                        sig[i] = RtcmV3Helper.msm_sig_gps[Signals[i] - 1];
                        break;
                    case NavigationSystemEnum.SYS_SBS:
                        // SYS_SBS: msm_type=q=rtcm->msmtype[4];
                        sig[i] = RtcmV3Helper.msm_sig_sbs[Signals[i] - 1];
                        break;
                    case NavigationSystemEnum.SYS_GLO:
                        // msm_type=q=rtcm->msmtype[1];
                        sig[i] = RtcmV3Helper.msm_sig_glo[Signals[i] - 1];
                        break;
                    case NavigationSystemEnum.SYS_GAL:
                        // msm_type=q=rtcm->msmtype[2];
                        sig[i] = RtcmV3Helper.msm_sig_gal[Signals[i] - 1];
                        break;
                    case NavigationSystemEnum.SYS_QZS:
                        // msm_type=q=rtcm->msmtype[3];
                        sig[i] = RtcmV3Helper.msm_sig_qzs[Signals[i] - 1];
                        break;
                    case NavigationSystemEnum.SYS_CMP:
                        // SYS_CMP: msm_type=q=rtcm->msmtype[5];
                        sig[i] = RtcmV3Helper.msm_sig_cmp[Signals[i] - 1];
                        break;
                    case NavigationSystemEnum.SYS_IRN:
                        // msm_type = q = rtcm->msmtype[6];
                        sig[i] = RtcmV3Helper.msm_sig_irn[Signals[i] - 1];
                        break;
                    default:
                        sig[i] = "";
                        break;
                }

                /* signal to rinex obs type */
                code[i] = RtcmV3Helper.Obs2Code(sig[i]);
                idx[i] = RtcmV3Helper.Code2Idx(sys, code[i]);


                if (code[i] != RtcmV3Helper.CODE_NONE)
                {
                    // if (q)
                    q += $"L{sig[i]}{(i < Signals.Length - 1 ? ", " : "")}";
                }
                else
                {
                    // if (q)
                    q += $"({Signals[i]}){(i < Signals.Length - 1 ? "," : "")}";
                }
            }

            /* get signal index */
            RtcmV3Helper.sigindex(sys, code, Signals.Length, "", idx);

            var obs = new List<MsmItem>();
            
            for (var i = 0; i < Satellites.Length; i++)
            {
                var j = 0;
                var prn = Satellites[i];
                if (sys == NavigationSystemEnum.SYS_QZS) prn += RtcmV3Helper.MINPRNQZS - 1;
                else if (sys == NavigationSystemEnum.SYS_SBS) prn += RtcmV3Helper.MINPRNSBS - 1;
                int sat;
                if ((sat = RtcmV3Helper.satno(sys, prn)) != 0)
                {
                    if (ObservableDataIsComplete)
                    {
                        ObservableDataIsComplete = false;
                    }
                }
                
                var obsItem = new MsmItem();
                var pseudoRanges = new List<double>();
                var carrierPhases = new List<double>();
                var codes = new List<byte>();
                var snrs = new List<ushort>();
                for (var k = 0; k < Signals.Length; k++)
                {
                    if (CellMask[k + i * Signals.Length] == 0) continue;

                    if (sat != 0 && idx[k] >= 0)
                    {
                        /* pseudorange (m) */
                        if (r[i] != 0.0 && pr[j] > -1E12)
                        {
                            pseudoRanges.Add(r[i] + pr[j]);
                        }
                        /* carrier-phase (cycle) */
                        if (r[i] != 0.0 && cp[j] > -1E12)
                        {
                            carrierPhases.Add((r[i] + cp[j]) * RtcmV3Helper.FREQ1 / RtcmV3Helper.CLIGHT);
                        }
                        
                        snrs.Add((ushort)(cnr[j] / RtcmV3Helper.SNR_UNIT + 0.5));
                        codes.Add(code[k]);
                    }
                    j++;
                }

                obsItem.CarrierPhase = carrierPhases.ToArray();
                obsItem.PseudoRange = pseudoRanges.ToArray();
                obsItem.Code = codes.ToArray();
                obsItem.Snr = snrs.ToArray();
                obsItem.Time = EpochTime;
                obs.Add(obsItem);
            }
            ObsSatellites = obs.ToArray();

        }

        public MsmItem[] ObsSatellites { get; set; }

        public override ushort MessageId { get; }

    }

    public class MsmItem
    {
        public DateTime Time { get; set; }

        /// <summary>
        /// Signal strength (0.001 dBHz)
        /// </summary>
        public ushort[] Snr { get; set; }

        /// <summary>
        /// Code indicator (CODE_???)
        /// </summary>
        public byte[] Code { get; set; }

        /// <summary>
        /// Observation data carrier-phase (cycle)
        /// </summary>
        public double[] CarrierPhase { get; set; }

        /// <summary>
        /// Observation data PseudoRange (m)
        /// </summary>
        public double[] PseudoRange { get; set; }
    }
}