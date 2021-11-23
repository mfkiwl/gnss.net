using System;

namespace Asv.Gnss
{
    public enum AntennaId
    {
        Main = 0,
        Aux1 = 1,
        Aux2 = 2
    }

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
            var offset = (int)offsetInBytes;
            RxChannel = buffer[offset]; offset += 1;
            TypeBitfield = buffer[offset]; offset += 1;
            SignalNumber = (TypeBitfield & 0b00011111);
            Antenna = (AntennaId)(TypeBitfield >> 5);
            SVID = buffer[offset]; offset += 1;
            Misc = buffer[offset]; offset += 1;
            CodeLSB = BitConverter.ToUInt32(buffer, offset); offset += 4;
            PRtype1 = ((Misc & 0b00000111) * 4294967296 + CodeLSB) * 0.001;
            DopplerHz = BitConverter.ToInt32(buffer, offset) * 0.0001; offset += 4;
            CarrierLSB = BitConverter.ToUInt16(buffer, offset); offset += 2;
            CarrierMSB = (sbyte)buffer[offset]; offset += 1;
            // SbfHelper.GetSignalType(SignalNumber,)
            // FullCarrierPhase = PRtype1 / (299792458 / Freq) + (CarrierMSB * 65536 + CarrierLSB) * 0.001;
            //TODO: implement block deserialization
            return 0;
        }

        public int? SignalNumber { get; set; }

        public AntennaId Antenna { get; set; }

        /// <summary>
        /// MSB of the carrier phase relative to the pseudorange. The full carrier phase can be computed by:
        /// L[cycles] = PRtype1[m]/λ +(CarrierMSB*65536+CarrierLSB)*0.001
        /// where λ is the carrier wavelength corresponding to the
        /// frequency of the signal type in the Type field above:
        /// λ=299792458/fL m, with fL the carrier frequency as listed
        /// in section 4.1.10.
        /// </summary>
        public double FullCarrierPhase { get; set; }

        public sbyte CarrierMSB { get; set; }

        public ushort CarrierLSB { get; set; }

        public double DopplerHz { get; set; }
        /// <summary>
        /// SB of the pseudorange. The pseudorange expressed in meters
        /// is computed as follows: PRtype1[m] = (CodeMSB*4294967296+CodeLSB)*0.001
        /// where CodeMSB is part of the Misc field
        /// </summary>
        public double PRtype1 { get; set; }

        public uint CodeLSB { get; set; }

        public byte Misc { get; set; }

        public byte SVID { get; set; }

        public byte TypeBitfield { get; set; }

        public byte RxChannel { get; set; }
    }

    public class SbfPacketMeasEpoch : SbfPacketBase
    {
        public override ushort MessageType => 4027;
        public override ushort MessageRevision => 0;

        protected override void DeserializeMessage(byte[] buffer, uint offsetBits)
        {
            var offset = offsetBits / 8;
            N1 = buffer[offset]; offset+=1;
            SB1Length = buffer[offset]; offset += 1;
            SB2Length = buffer[offset]; offset += 1;
            CommonFlags = (MeasEpochCommonFlags) buffer[offset]; offset += 1;
            if (MessageRevision == 1)
            {
                CumClkJumps = buffer[offset] * 0.001; offset += 1;
            }
            Reserved = buffer[offset]; offset += 1;
            SubBlocks = new MeasEpochChannelType1[N1];
            for (var index = 0; index < SubBlocks.Length; index++)
            {
                SubBlocks[index] = new MeasEpochChannelType1();
                offset += SubBlocks[index].Deserialize(buffer, offset, SB1Length);
            }
        }
        /// <summary>
        /// Cumulative millisecond clock jumps since start-up, with an ambiguity of
        /// k*256 ms.For example, if two clock jumps of -1 ms have occurred since
        /// startup, this field contains the value 254.
        /// </summary>
        public double CumClkJumps { get; set; }

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
}