using System;
using System.IO;
using System.Text;

namespace Asv.Gnss
{
    public enum AsvDeviceType : ushort
    {
        Unknown = 0,
        GbasServer = 1,
        GbasModulator = 2,
        GbasMonDev = 3,
    }

    public enum AsvDeviceState : byte
    {
        Unknown = 0,
        Active = 1,
        Error = 2,
    }


    public class AsvMessageHeartBeat : AsvMessageBase
    {
        public static ushort PacketMessageId = 0x0001;

        public override ushort MessageId => PacketMessageId;

        public AsvDeviceType DeviceType { get; set; }
        public AsvDeviceState DeviceState { get; set; }

        protected override int InternalSerialize(byte[] buffer, int offsetInBytes)
        {
            using (var strm = new BinaryWriter(new MemoryStream(buffer, offsetInBytes, buffer.Length - offsetInBytes), Encoding.ASCII, false))
            {
                strm.Write((ushort)DeviceType);
                strm.Write((byte)DeviceState);
                strm.Write((uint)0);
            }
            return 7;
        }

        protected override int InternalDeserialize(byte[] buffer, int offsetInBytes, int length)
        {
            DeviceType = (AsvDeviceType) BitConverter.ToUInt16(buffer, offsetInBytes);
            DeviceState = (AsvDeviceState)buffer[offsetInBytes + 2];
            return 6; // full size heartbeat with 
        }

        
    }
}