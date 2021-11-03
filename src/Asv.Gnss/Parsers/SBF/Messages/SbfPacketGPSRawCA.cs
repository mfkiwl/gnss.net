using System;

namespace Asv.Gnss
{
    /// <summary>
    /// This block contains the 300 bits of a GPS C/A subframe. It is generated each time a new
    /// subframe is received, i.e.every 6 seconds.
    /// </summary>
    public class SbfPacketGPSRawCA : SbfPacketBase
    {
        public override ushort MessageId => 4017;

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
            NAVBits = new byte[40];
            Array.Copy(buffer,startIndex + 5,NAVBits,0,40);
            //Padding
        }
        

        public SbfNavSysEnum NavSystem { get; set; }

        /// <summary>
        /// NAVBits contains the 300 bits of a GPS C/A subframe.
        /// Encoding: For easier parsing, the bits are stored as a succession of
        /// 10 32-bit words.Since the actual words in the subframe are 30-bit long,
        /// two unused bits are inserted in each 32-bit word. More specifically, each
        /// 32-bit word has the following format:
        /// Bits 0-5: 6 parity bits (referred to as D25 to D30 in the GPS ICD), XOR-ed
        /// with the last transmitted bit of the previous word(D∗ 30)).
        /// Bits 6-29: source data bits(referred to as dn in the GPS ICD). The first
        /// received bit is the MSB.
        /// Bits 30-31: Reserved
        /// </summary>
        public byte[] NAVBits { get; set; }

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
    }
}