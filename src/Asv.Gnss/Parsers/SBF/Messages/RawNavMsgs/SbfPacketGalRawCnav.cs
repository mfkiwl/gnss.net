﻿namespace Asv.Gnss
{
    /// <summary>
    /// This block contains the 492 bits of a Galileo C/NAV navigation page, after deinterleaving and Viterbi decoding.
    ///
    /// NAVBits contains the 492 bits of a Galileo C/NAV page.
    /// Encoding: NAVBits contains all the bits of the frame, with the exception of the synchronization field. The first received bit is stored as
    /// the MSB of NAVBits[0]. The unused bits in NAVBits[15] must be
    /// ignored by the decoding software.
    /// </summary>
    public class SbfPacketGalRawCnav : SbfPacketGnssRawNavMsgBase
    {
        public override ushort MessageId => 4024;

        protected override int NavBytesLength => 64;

    }
}