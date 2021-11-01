using System;
using System.IO;
using System.Text;

namespace Asv.Gnss
{
    [Flags]
    public enum AsvGbasSlot:byte
    {
        SlotA = 0b00000000,
        SlotB = 0b00000010,
        SlotC = 0b00000100,
        SlotD = 0b00001000,
        SlotE = 0b00010000,
        SlotF = 0b00100000,
        SlotG = 0b01000000,
        SlotH = 0b10000000,
    }

    [Flags]
    public enum AsvGbasMessage : ulong
    {
        Msg1 = 0b00000000,
        Msg101 = 0b00000010,
        Msg2 = 0b00000100,
        Msg3 = 0b00001000,
        Msg4 = 0b00010000,
        Msg5 = 0b00100000,
    }

    public class AsvMessageGbasVdbSend : AsvMessageBase
    {
        public override ushort MessageId => 0x0100;

        protected override int InternalSerialize(byte[] buffer, int offsetInBytes)
        {
            using (var strm = new BinaryWriter(new MemoryStream(buffer, offsetInBytes, buffer.Length - offsetInBytes), Encoding.ASCII, false))
            {
                strm.Write((byte)Slot);
                strm.Write((ulong)Msgs);
                strm.Write(LastByteLength);
                strm.Write(Data);
            }
            return Data.Length + 10;
        }

        protected override int InternalDeserialize(byte[] buffer, int offsetInBytes, int length)
        {
            Slot = (AsvGbasSlot)buffer[offsetInBytes];
            Msgs = (AsvGbasMessage) BitConverter.ToUInt64(buffer, offsetInBytes + 1);
            LastByteLength = buffer[offsetInBytes + 9];
            Data = new byte[length - 10];
            Array.Copy(buffer, offsetInBytes + 10, Data, 0, Data.Length);
            
            return length; // full size heartbeat with 
        }
        public byte[] Data { get; set; }
        public byte LastByteLength { get; set; }
        public AsvGbasMessage Msgs { get; set; }
        public AsvGbasSlot Slot { get; set; }
    }
}