using System;

namespace Asv.Gnss
{
    public class UbxHotHardwareReset : UbxResetReceiver
    {
        public UbxHotHardwareReset()
        {
            Bbr = BbrMask.HotStart;
            Mode = ResetMode.HardwareResetImmediately;
        }
    }

    public class UbxColdSoftwareGnssReset : UbxResetReceiver
    {
        public UbxColdSoftwareGnssReset()
        {
            Bbr = BbrMask.ColdStart;
            Mode = ResetMode.ControlledSoftwareResetGnssOnly;
        }
    }


    public enum BbrMask : ushort
    {
        HotStart = 0x00,
        WarmStart = 0x01,
        ColdStart = 0xFF14
    }

    public enum ResetMode : byte
    {
        HardwareResetImmediately = 0x00,
        ControlledSoftwareReset = 0x01,
        ControlledSoftwareResetGnssOnly = 0x02,
        HardwareResetAfterShutdown = 0x04,
        ControlledGnssStop = 0x08,
        ControlledGnssStart = 0x09
    }

    public class UbxResetReceiver : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x04;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 4;
        }

        public BbrMask Bbr { get; set; } = BbrMask.HotStart;

        public ResetMode Mode { get; set; } = ResetMode.HardwareResetImmediately;

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            var bbr = BitConverter.GetBytes((ushort)Bbr);
            buffer[byteIndex++] = bbr[0];
            buffer[byteIndex++] = bbr[1];
            buffer[byteIndex++] = (byte)Mode;
            buffer[byteIndex++] = 0;

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            Bbr = (BbrMask)BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            Mode = (ResetMode)buffer[byteIndex]; byteIndex += 2;

            return byteIndex * 8 - offsetBits;
        }
    }
}