namespace Asv.Gnss
{
    public class RtcmV3Preamble
    {
        public const byte SyncByte = 0xD3;

        public void Deserialize(byte[] buffer)
        {
            uint i = 0;

            Preamble = (byte)BitOperationHelper.GetBitU(buffer, i, 8);
            i += 8;
            Reserved = (byte)BitOperationHelper.GetBitU(buffer, i, 6);
            i += 6;
            PacketLength = (ushort)BitOperationHelper.GetBitU(buffer, i, 10);
            i += 10;
        }

        public void Serialize(byte[] buffer)
        {
            uint i = 0;

            BitOperationHelper.SetBitU(buffer, i, 8, SyncByte);
            i += 8;
            BitOperationHelper.SetBitU(buffer, i, 6, Reserved);
            i += 6;
            BitOperationHelper.SetBitU(buffer, i, 10, PacketLength);
            i += 10;
        }

        public byte Reserved { get; set; }
        public byte Preamble { get; private set; }
        public uint PacketLength { get; set; }
    }
}