using System;

namespace Asv.Gnss
{
    /// <summary>
    /// Bit field containing flags common to all measurements.
    /// </summary>
    [Flags]
    public enum MeasEpochCommonFlags : byte
    {
        /// <summary>
        /// Bit 0: Multipath mitigation: if this bit is set, multipath mitigation is enabled. (see the setMultipathMitigation command).
        /// </summary>
        MultipathMitigationEnabled,
        /// <summary>
        /// Bit 1: Smoothing of code: if this bit is set, at least one of the code measurements are smoothed values (see setSmoothingInterval command).
        /// </summary>
        MeasurementsAreSmoothed,
        /// <summary>
        /// Bit 2: Carrier phase align: if this bit is set, the fractional part of the carrier phase measurements from different modulations on the same
        /// carrier frequency (e.g. GPS L2C and L2P) are aligned, i.e. multiplexing biases (0.25 or 0.5 cycles) are corrected. Aligned carrier phase
        /// measurements can be directly included in RINEX files. If this bit is
        /// unset, this block contains raw carrier phase measurements. This bit
        /// is always set in the current firmware version.
        /// </summary>
        CarrierPhaseAlign,
        /// <summary>
        /// Bit 3: Clock steering: this bit is set if clock steering is active (seesetClockSyncThreshold command).
        /// </summary>
        ClockSteering,
        /// <summary>
        /// Bit 4: Not applicable.
        /// </summary>
        NotApplicable,
        /// <summary>
        /// Bit 5: High dynamics: this bit is set when the receiver is in high-dynamics mode (see the setReceiverDynamics command).
        /// </summary>
        HighDynamics,
        /// <summary>
        /// Bit 6: Reserved
        /// </summary>
        Reserved,
        /// <summary>
        /// Bit 7: Scrambling: bit set when the measurements are scrambled. Scrambling is applied when the "Measurement Availability" permission is
        /// not granted (see the lif, Permissions command)
        /// </summary>
        Scrambling,
    }

    public class MeasEpochChannelType1
    {
        public uint Deserialize(byte[] buffer, uint offsetInBytes, uint blockLength)
        {
            //TODO: implement block deserialization
            return 0;
        }
    }

    public class SbfPacketMeasEpoch : SbfPacketBase
    {
        public override ushort MessageId => 4027;

        protected override void DeserializeMessage(byte[] buffer, uint offsetBits)
        {
            uint offsetInBytes = offsetBits / 8;
            N1 = buffer[offsetInBytes]; offsetInBytes+=1;
            SB1Length = buffer[offsetInBytes]; offsetInBytes += 1;
            SB2Length = buffer[offsetInBytes]; offsetInBytes += 1;
            CommonFlags = (MeasEpochCommonFlags) buffer[offsetInBytes]; offsetInBytes += 1;
            Reserved = buffer[offsetInBytes]; offsetInBytes += 1;
            SubBlocks = new MeasEpochChannelType1[N1];
            foreach (var measEpochChannelType1 in SubBlocks)
            {
                offsetInBytes+= measEpochChannelType1.Deserialize(buffer, offsetInBytes, SB1Length);
            }

        }

        public MeasEpochChannelType1[] SubBlocks { get; set; }

        /// <summary>
        /// Reserved for future use, to be ignored by decoding software
        /// </summary>
        public byte Reserved { get; set; }


        public MeasEpochCommonFlags CommonFlags { get; set; }
        /// <summary>
        /// Length of a MeasEpochChannelType2 sub-block
        /// </summary>
        public byte SB2Length { get; set; }
        /// <summary>
        /// Length of a MeasEpochChannelType1 sub-block, excluding the nested MeasEpochChannelType2 sub-blocks
        /// </summary>
        public byte SB1Length { get; set; }

        /// <summary>
        /// Number of MeasEpochChannelType1 sub-blocks in this MeasEpoch block
        /// </summary>
        public byte N1 { get; set; }
    }

    public abstract class SbfPacketBase: GnssMessageBase
    {
        public override string ProtocolId => SbfBinaryParser.GnssProtocolId;

        public abstract ushort MessageId { get; }
        public ushort MessageRevision { get; protected set; }

        public override int GetMaxByteSize()
        {
            return ComNavBinaryParser.MaxPacketSize;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var offsetInBytes = (int)(offsetBits / 8);
            var msgId = BitConverter.ToUInt16(buffer, offsetInBytes + 4);
            var msgLength = BitConverter.ToUInt16(buffer, offsetInBytes + 6);
            var type = msgId & 0x1fff << 0;
            MessageRevision = (ushort) (msgId >> 13);

            if (type != MessageId) throw new GnssParserException(ComNavBinaryParser.GnssProtocolId, $"Error to deserialize SBF packet message. Id not equal (want [{MessageId}] read [type:{type}])");
            TOW = BitConverter.ToUInt32(buffer, offsetInBytes + 8);
            WNc = BitConverter.ToUInt16(buffer, offsetInBytes + 12);

            UtcTime = new DateTime(1980,1,06).AddMilliseconds(TOW + WNc * 604800000 /* ms in 1 week */);
            DeserializeMessage(buffer, offsetBits + 14 * 8U);
            return (4U + msgLength ) * 8U;
        }

        

        protected abstract void DeserializeMessage(byte[] buffer, uint offsetBits);

        public DateTime UtcTime { get; set; }
        /// <summary>
        /// Time-Of-Week : Time-tag, expressed in whole milliseconds from the beginning of the current GPS week.
        /// </summary>
        public uint TOW { get; set; }
        /// <summary>
        /// The GPS week number associated with the TOW. WNc is a continuous week count(hence the "c").
        /// It is not affected by GPS week rollovers, which occur every 1024 weeks.
        /// By definition of the Galileo system time, WNc is also the Galileo week number plus 1024.
        /// </summary>
        public ushort WNc { get; set; }
    }
}