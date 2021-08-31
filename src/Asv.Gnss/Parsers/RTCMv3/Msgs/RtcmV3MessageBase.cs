namespace Asv.Gnss
{
    public abstract class RtcmV3MessageBase:ISerializable
    {
        public RtcmV3MessageBase(RtcmV3Preamble preamble, RtcmV3Header header)
        {
            Preamble = preamble;
            Header = header;
        }

        public RtcmV3Preamble Preamble { get; }
        public RtcmV3Header Header { get; }
        public abstract void Serialize(byte[] buffer, uint startIndex = 0);
        public abstract void Deserialize(byte[] buffer, uint startIndex = 0);
    }
}