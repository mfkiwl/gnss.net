namespace Asv.Gnss
{
    public class RtcmV3Preamble:ISerializable
    {
        public const byte SyncByte = 0xD3;

        public void Deserialize(byte[] buffer, uint startIndex = 0)
        {
            uint bitIndex = 0;

            Preamble = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8);
            bitIndex += 8;
            Reserved = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 6);
            bitIndex += 6;
            PacketLength = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 10);
            bitIndex += 10;
        }

        public void Serialize(byte[] buffer, uint startIndex = 0)
        {
            uint i = 0;

            RtcmV3Helper.SetBitU(buffer, i, 8, SyncByte);
            i += 8;
            RtcmV3Helper.SetBitU(buffer, i, 6, Reserved);
            i += 6;
            RtcmV3Helper.SetBitU(buffer, i, 10, PacketLength);
            i += 10;
        }

        public byte Reserved { get; set; }
        public byte Preamble { get; private set; }
        public uint PacketLength { get; set; }
       
}