namespace Asv.Gnss
{
    public abstract class AsvMessageBase:GnssMessageBaseWithId<ushort>
    {
        public override string ProtocolId => AsvParser.GnssProtocolId;

        public override int GetMaxByteSize()
        {
            return 1024;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

    }
}