using System;

namespace Asv.Gnss
{
    public abstract class SbfPacketGnssRawNavMsgBase : SbfPacketBase
    {


        public SbfNavSysEnum NavSystem { get; set; }

        public uint[] NAVBitsU32 { get; set; }

        /// <summary>
        /// Receiver channel (see 4.1.11)
        /// </summary>
        public byte RxChannel { get; set; }

        /// <summary>
        /// GLONASS frequency number, with an offset of 8.
        /// It ranges from 1 (corresponding to an actual frequency number of -7) to 21 (corresponding to an
        /// actual frequency number of 13).
        /// For non-GLONASS satellites, FreqNr is reserved
        /// and must be ignored by the decoding software.
        /// </summary>
        public byte FreqNr { get; set; }

        /// <summary>
        /// Bit field:
        /// Bits 0-4: Signal type from which the bits have been received, as defined
        /// in 4.1.10
        /// Bits 5-7: Reserved
        /// </summary>
        public byte Source { get; set; }

        /// <summary>
        /// Not applicable
        /// </summary>
        public byte ViterbiCnt { get; set; }

        /// <summary>
        /// Status of the CRC or parity check:
        /// 0: CRC or parity check failed
        /// 1: CRC or parity check passed
        /// </summary>
        public bool CrcPassed { get; set; }

        /// <summary>
        /// Satellite ID, see 4.1.9
        /// </summary>
        public byte SvId { get; set; }

        /// <summary>
        /// RINEX satellite code
        /// </summary>
        public string RinexSatCode { get; set; }

        public SbfSignalTypeEnum SignalType { get; set; }

        public string RindexSignalCode { get; set; }

        public double CarrierFreq { get; set; }

        protected abstract int NavBitsU32Length { get; }

        protected override void DeserializeMessage(byte[] buffer, uint offsetBits)
        {
            var startIndex = offsetBits / 8;

            SvId = buffer[startIndex];
            RinexSatCode = SbfHelper.GetRinexSatteliteCode(SvId, out var nav);
            SatPrn = SbfHelper.GetSattelitePrn(SvId);
            NavSystem = nav;
            CrcPassed = buffer[startIndex + 1] != 0;
            ViterbiCnt = buffer[startIndex + 2];
            Source = buffer[startIndex + 3];
            FreqNr = buffer[startIndex + 4];
            SignalType = SbfHelper.GetSignalType(Source, FreqNr, out var constellation, out var carrierFreq, out var signalRinexCode);

            if (constellation != NavSystem) throw new Exception("Navigation system code not euqals");
            CarrierFreq = carrierFreq;
            RindexSignalCode = signalRinexCode;
            RxChannel = buffer[startIndex + 5];
            NAVBitsU32 = new uint[NavBitsU32Length];
            for (var i = 0; i < NavBitsU32Length; i++)
            {
                var index = (int) (startIndex + 6 + i * 4);
                NAVBitsU32[i] = BitConverter.ToUInt32(buffer, index);
            }
            //Padding ignored
        }

        public int SatPrn { get; set; }
    }
}