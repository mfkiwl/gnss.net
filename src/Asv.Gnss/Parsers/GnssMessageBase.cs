namespace Asv.Gnss
{
    public abstract class GnssMessageBase : ISerializable
    {
        public string ProtocolId { get; }
        public abstract int GetMaxByteSize();
        public abstract uint Serialize(byte[] buffer, uint offsetBits);
        public abstract uint Deserialize(byte[] buffer, uint offsetBits);
    }
}