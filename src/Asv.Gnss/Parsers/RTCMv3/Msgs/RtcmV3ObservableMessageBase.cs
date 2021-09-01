namespace Asv.Gnss
{
    public abstract class RtcmV3ArpMessageBase : ISerializable
    {
        public RtcmV3ArpMessageBase(RtcmV3Preamble preamble, RtcmV3HeaderBase header)
        {
            Preamble = preamble;
            Header = header;
        }

        public RtcmV3Preamble Preamble { get; }
        public RtcmV3HeaderBase Header { get; }
        public abstract void Serialize(byte[] buffer, uint startIndex = 0);
        public abstract void Deserialize(byte[] buffer, uint startIndex = 0);
    }

    public abstract class RtcmV3ObservableMessageBase:ISerializable
    {
        public RtcmV3ObservableMessageBase(RtcmV3Preamble preamble, RtcmV3ObservableHeader header)
        {
            Preamble = preamble;
            Header = header;
        }

        public RtcmV3Preamble Preamble { get; }
        public RtcmV3ObservableHeader Header { get; }
        public abstract void Serialize(byte[] buffer, uint startIndex = 0);
        public abstract void Deserialize(byte[] buffer, uint startIndex = 0);
    }
}