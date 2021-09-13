using System;
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
            var nCell = CellMask.SelectMany(_ => _).Count(_ => _ > 0);

            // Satellite data Nsat*(8 + 10) bit
            // Satellite  rough ranges
            var roughRanges = new double[SatelliteIds.Length];

            // Signal data
            // Pseudoranges 15*Ncell
            var pseudorange = new double[nCell];
            // PhaseRange data 22*Ncell
            var phaseRange = new double[nCell];
            // signal CNRs 6*Ncell
            var cnr = new double[nCell];

            //PhaseRange LockTime Indicator 4*Ncell
            var @lock = new byte[nCell];
            //Half-cycle ambiguityindicator 1*Ncell
            var halfCycle = new byte[nCell];

            for (var i = 0; i < SatelliteIds.Length; i++) roughRanges[i] = 0.0;
            for (var i = 0; i < nCell; i++) pseudorange[i] = phaseRange[i] = -1E16;

            /* decode satellite data, rough ranges */
            for (var i = 0; i < SatelliteIds.Length; i++)
            {
                /* Satellite  rough ranges */
                var rng = RtcmV3Helper.GetBitU(buffer, bitIndex, 8);
                bitIndex += 8;
                if (rng != 255) roughRanges[i] = rng * RtcmV3Helper.RANGE_MS;
            }

            for (var i = 0; i < SatelliteIds.Length; i++)
            {
                var rngM = RtcmV3Helper.GetBitU(buffer, bitIndex, 10);
                bitIndex += 10;
                if (roughRanges[i] != 0.0) roughRanges[i] += rngM * RtcmV3Helper.P2_10 * RtcmV3Helper.RANGE_MS;
            }

            /* decode signal data */
            for (var i = 0; i < nCell; i++)
            {
                /* pseudorange */
                var prv = RtcmV3Helper.GetBits(buffer, bitIndex, 15);
                bitIndex += 15;
                if (prv != -16384) pseudorange[i] = prv * RtcmV3Helper.P2_24 * RtcmV3Helper.RANGE_MS;
            }

            for (var i = 0; i < nCell; i++)
            {
                /* phase range */
                var cpv = RtcmV3Helper.GetBits(buffer, bitIndex, 22);
                bitIndex += 22;
                if (cpv != -2097152) phaseRange[i] = cpv * RtcmV3Helper.P2_29 * RtcmV3Helper.RANGE_MS;
            }

            for (var i = 0; i < nCell; i++)
            {
                /* lock time */
                @lock[i] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 4);
                bitIndex += 4;
            }

            for (var i = 0; i < nCell; i++)
            {
                /* half-cycle ambiguity */
                halfCycle[i] = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;
            }

            for (var i = 0; i < nCell; i++)
            {
                /* cnr */
                /* GNSS signal CNR
                 1–63 dBHz */
                cnr[i] = RtcmV3Helper.GetBitU(buffer, bitIndex, 6) * 1.0; bitIndex += 6;
            }

            CreateMsmObservable(roughRanges, pseudorange, phaseRange, @lock, halfCycle, cnr);

            return bitIndex;
        }

        private void CreateMsmObservable(double[] roughRanges, double[] pseudorange, double[] phaseRange, byte[] @lock, byte[] halfCycle, double[] cnr)
        {
            
            
            var q = "";
            
            var sys = RtcmV3Helper.GetNavigationSystem(MessageId);
            Satellites = new Satellite[SatelliteIds.Length];
            for (var i = 0; i < SatelliteIds.Length; i++)
            {
                var idx = new int[SignalIds.Length];
                var code = new byte[SignalIds.Length];
                var satellite = new Satellite
                {
                    SatellitePrn = SatelliteIds[i], 
                    Signals = new Signal[CellMask[i].Count(_ => _ != 0)]
                };
                Satellites[i] = satellite;
                var index = 0;
                for (var j = 0; j < SignalIds.Length; j++)
                {
                    if (CellMask[i][j] == 0) continue;
                    Satellites[i].Signals[index] = new Signal();
                    switch (sys)
                    {
                        case NavigationSystemEnum.SYS_GPS: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_gps[SignalIds[j] - 1]; break;
                        case NavigationSystemEnum.SYS_GLO: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_glo[SignalIds[j] - 1]; break;
                        case NavigationSystemEnum.SYS_GAL: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_gal[SignalIds[j] - 1]; break;
                        case NavigationSystemEnum.SYS_QZS: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_qzs[SignalIds[j] - 1]; break;
                        case NavigationSystemEnum.SYS_SBS: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_sbs[SignalIds[j] - 1]; break;
                        case NavigationSystemEnum.SYS_CMP: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_cmp[SignalIds[j] - 1]; break;
                        case NavigationSystemEnum.SYS_IRN: satellite.Signals[index].RinexCode = RtcmV3Helper.msm_sig_irn[SignalIds[j] - 1]; break;
                        default: satellite.Signals[index].RinexCode = ""; break;
                    }

                    /* signal to rinex obs type */
                    code[j] = RtcmV3Helper.Obs2Code(satellite.Signals[index].RinexCode);
                    idx[j] = RtcmV3Helper.Code2Idx(sys, code[j]);

                    if (code[j] != RtcmV3Helper.CODE_NONE)
                    {
                        q += $"L{satellite.Signals[index].RinexCode}{(j < satellite.Signals.Length - 1 ? ", " : "")}";
                    }
                    else
                    {
                        q += $"({SignalIds[j]}){(j < satellite.Signals.Length - 1 ? ", " : "")}";
                    }

                    // try
                    // {
                    //     RtcmV3Helper.sigindex(sys, code, SignalIds.Length, "", idx);
                    // }
                    // catch (Exception e)
                    // {
                    //     
                    // }
                    /* get signal index */
                    

                    if (sys == NavigationSystemEnum.SYS_QZS) satellite.SatellitePrn += RtcmV3Helper.MINPRNQZS - 1;
                    else if (sys == NavigationSystemEnum.SYS_SBS) satellite.SatellitePrn += RtcmV3Helper.MINPRNSBS - 1;

                    var freq = 1.0;
                    if (idx[j] >= 0)
                    {
                        /* pseudorange (m) */
                        if (roughRanges[i] != 0.0 && pseudorange[j] > -1E12)
                        {
                            satellite.Signals[index].PseudoRange = roughRanges[i] + pseudorange[j];
                        }
                        /* carrier-phase (cycle) */
                        if (roughRanges[i] != 0.0 && phaseRange[j] > -1E12)
                        {
                            satellite.Signals[index].CarrierPhase = (roughRanges[i] + pseudorange[j]) * freq / RtcmV3Helper.CLIGHT;
                        }

                        // var sat = RtcmV3Helper.satno(sys, satellite.SatellitePrn);
                        // satellite.Signals[index].LockTime = lossoflock(rtcm, sat, idx[j], @lock[j]) + (halfCycle[j] != 0 ? 3 : 0);

                        satellite.Signals[index].Cnr = (ushort)(cnr[j] / RtcmV3Helper.SNR_UNIT + 0.5);
                        satellite.Signals[index].CodeId = code[j];
                        satellite.Signals[index].Id = SignalIds[j];
                    }

                    index++;
                }
            }
        }

        public Satellite[] Satellites { get; set; }

        public override ushort MessageId { get; }

    }


    public class Satellite
    {
        public byte SatellitePrn { get; set; }
        public Signal[] Signals { get; set; }
    }

    public class Signal
    {
        public byte Id { get; set; }
        public string FrequencyBand { get; set; }
        public string SignalName { get; set; }
        public string RinexCode { get; set; }
        
        /// <summary>
        /// Observation data PseudoRange (m)
        /// </summary>
        public double PseudoRange { get; set; }

        /// <summary>
        /// Observation data carrier-phase (cycle)
        /// </summary>
        public double CarrierPhase { get; set; }

        /// <summary>
        /// Signal strength (0.001 dBHz)
        /// </summary>
        public ushort Cnr { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte LockTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte HalfCycle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public byte CodeId { get; set; }
    }

}