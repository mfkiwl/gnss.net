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
        /// Not applicable
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

        protected abstract int NavBitsU32Length { get; }

        protected override void DeserializeMessage(byte[] buffer, uint offsetBits)
        {
            var startIndex = offsetBits / 8;

            SvId = buffer[startIndex];
            RinexSatCode = SbfHelper.GetRinexSatteliteCode(SvId, out var nav);
            NavSystem = nav;
            CrcPassed = buffer[startIndex + 1] != 0;
            ViterbiCnt = buffer[startIndex + 2];
            Source = buffer[startIndex + 3];
            FreqNr = buffer[startIndex + 4];
            RxChannel = buffer[startIndex + 5];
            NAVBitsU32 = new uint[NavBitsU32Length];
            for (var i = 0; i < NavBitsU32Length; i++)
            {
                var index = (int) (startIndex + 6 + i * 4);
                NAVBitsU32[i] = BitConverter.ToUInt32(buffer, index);
            }
            //Padding ignored
        }
    }
}