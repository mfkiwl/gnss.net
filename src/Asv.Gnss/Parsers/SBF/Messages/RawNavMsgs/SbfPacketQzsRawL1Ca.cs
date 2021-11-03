namespace Asv.Gnss
{
    /// <summary>
    /// This block contains the 300 bits of a QZSS C/A subframe.
    /// 
    /// NAVBits contains the 300 bits of a QZSS C/A subframe.
    /// Encoding: Same as GPSRawCA block
    /// </summary>
    public class SbfPacketQzsRawL1Ca : SbfPacketGnssRawNavMsgBase
    {
        public override ushort MessageId => 4066;

        protected override int NavBytesLength => 40;

    }
}