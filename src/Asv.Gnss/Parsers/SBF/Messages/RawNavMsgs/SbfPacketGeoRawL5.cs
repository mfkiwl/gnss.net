﻿namespace Asv.Gnss
{
    /// This block contains the 250 bits of a SBAS L5 navigation frame, after Viterbi decoding
    ///
    /// NAVBits contains the 250 bits of a SBAS navigation frame.
    /// Encoding: NAVBits contains all the bits of the frame, including the
    /// preamble. The first received bit is stored as the MSB of NAVBits[0].
    /// The unused bits in NAVBits[7] must be ignored by the decoding
    /// software.
    /// </summary>
    public class SbfPacketGeoRawL5 : SbfPacketGnssRawNavMsgBase
    {
        public override ushort MessageId => 4021;

        protected override int NavBytesLength => 32;

    }
}