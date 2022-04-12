
namespace Asv.Gnss
{
    public class RawRtcmV3Message : GnssRawPacket<ushort>
    {
        public override string ProtocolId => RtcmV3Parser.GnssProtocolId;
        
        public RawRtcmV3Message(ushort messageId) : base(messageId)
        {
        }
    }
}
