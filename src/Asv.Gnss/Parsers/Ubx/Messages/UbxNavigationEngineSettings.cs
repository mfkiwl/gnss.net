using System;

namespace Asv.Gnss
{
    public class UbxNavigationEngineSettings : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x24;

        public bool ApplyDynamicModel { get; set; } = true;
        public bool ApplyMinimumElevation { get; set; } = true;
        public bool ApplyFixMode { get; set; } = true;
        public bool ApplyDrLimit { get; set; } = true;
        public bool ApplyPositionMask { get; set; } = true;
        public bool ApplyTimeMask { get; set; } = true;
        public bool ApplyStaticHold { get; set; } = true;
        public bool ApplyDGPS { get; set; } = true;
        public bool ApplyCnoThreshold { get; set; } = true;
        public bool ApplyUTC { get; set; } = true;
        

        public enum ModelEnum
        {
            Portable = 0,
            Stationary = 2,
            Pedestrian = 3,
            Automotive = 4,
            Sea = 5,
            AirborneWithLess1gAcceleration = 6,
            AirborneWithLess2gAcceleration = 7,
            AirborneWithLess4gAcceleration = 8,
            WristWornWatch = 9,
            Bike = 10
        }

        public ModelEnum PlatformModel { get; set; } = ModelEnum.Stationary;

        public enum PositionModeEnum
        {
            Only2D = 1,
            Only3D = 2,
            Auto = 3
        }

        public PositionModeEnum PositionMode { get; set; } = PositionModeEnum.Auto;

        public double FixedAltitude { get; set; }
        public double FixedAltitudeVariance { get; set; } = 1.0;
        public byte DrLimit { get; set; }
        public sbyte MinimumElevation { get; set; } = 15;
        public double PositionDOP { get; set; } = 25.0;
        public double TimeDOP { get; set; } = 25.0;
        public ushort PositionAccuracy { get; set; } = 100;
        public ushort TimeAccuracy { get; set; } = 300;
        public double StaticHoldThreshold { get; set; }
        public byte DGNSSTimeout { get; set; }
        public byte CnoThreshNumSVs { get; set; }
        public byte CnoThreshold { get; set; } = 35;
        public ushort StaticHoldMaxDistance { get; set; }
        public byte UtcStandard { get; set; }



        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 36;
        }

        public UbxNavigationEngineSettings(ModelEnum model)
        {
            PlatformModel = model;
            PositionMode = PositionModeEnum.Auto;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            ushort appliedBitMask = 0;
            if (ApplyDynamicModel) appliedBitMask |= 0x01;
            if (ApplyMinimumElevation) appliedBitMask |= 0x02;
            if (ApplyFixMode) appliedBitMask |= 0x04;
            if (ApplyDrLimit) appliedBitMask |= 0x08;
            if (ApplyPositionMask) appliedBitMask |= 0x10;
            if (ApplyTimeMask) appliedBitMask |= 0x20;
            if (ApplyStaticHold) appliedBitMask |= 0x40;
            if (ApplyDGPS) appliedBitMask |= 0x80;
            if (ApplyCnoThreshold) appliedBitMask |= 0x100;
            if (ApplyUTC) appliedBitMask |= 0x400;
            
            var appliedBitMaskBuff = BitConverter.GetBytes(appliedBitMask);
            buffer[byteIndex++] = appliedBitMaskBuff[0];
            buffer[byteIndex++] = appliedBitMaskBuff[1];

            buffer[byteIndex++] = (byte)PlatformModel;
            buffer[byteIndex++] = (byte)PositionMode;

            var fixedAltitude = BitConverter.GetBytes((int)Math.Round(FixedAltitude * 100));
            buffer[byteIndex++] = fixedAltitude[0];
            buffer[byteIndex++] = fixedAltitude[1];
            buffer[byteIndex++] = fixedAltitude[2];
            buffer[byteIndex++] = fixedAltitude[3];

            var fixedAltitudeVariance = BitConverter.GetBytes((uint)Math.Round(FixedAltitudeVariance * 10000));
            buffer[byteIndex++] = fixedAltitudeVariance[0];
            buffer[byteIndex++] = fixedAltitudeVariance[1];
            buffer[byteIndex++] = fixedAltitudeVariance[2];
            buffer[byteIndex++] = fixedAltitudeVariance[3];

            buffer[byteIndex++] = (byte)MinimumElevation;
            buffer[byteIndex++] = DrLimit;

            var positionDOP = BitConverter.GetBytes((ushort)Math.Round(PositionDOP * 10));
            buffer[byteIndex++] = positionDOP[0];
            buffer[byteIndex++] = positionDOP[1];

            var timeDOP = BitConverter.GetBytes((ushort)Math.Round(TimeDOP * 10));
            buffer[byteIndex++] = timeDOP[0];
            buffer[byteIndex++] = timeDOP[1];

            var positionAccuracy = BitConverter.GetBytes(PositionAccuracy);
            buffer[byteIndex++] = positionAccuracy[0];
            buffer[byteIndex++] = positionAccuracy[1];

            var timeAccuracy = BitConverter.GetBytes(TimeAccuracy);
            buffer[byteIndex++] = timeAccuracy[0];
            buffer[byteIndex++] = timeAccuracy[1];

            buffer[byteIndex++] = (byte)Math.Round(StaticHoldThreshold * 100);
            buffer[byteIndex++] = DGNSSTimeout;
            buffer[byteIndex++] = CnoThreshNumSVs;
            buffer[byteIndex] = CnoThreshold; byteIndex += 3;

            var staticHoldMaxDistance = BitConverter.GetBytes(StaticHoldMaxDistance);
            buffer[byteIndex++] = staticHoldMaxDistance[0];
            buffer[byteIndex++] = staticHoldMaxDistance[1];

            buffer[byteIndex] = UtcStandard; byteIndex += 6;

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            var appliedBitMask = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            ApplyDynamicModel = (appliedBitMask & 0x01) != 0;
            ApplyMinimumElevation = (appliedBitMask & 0x02) != 0;
            ApplyFixMode = (appliedBitMask & 0x04) != 0;
            ApplyDrLimit = (appliedBitMask & 0x08) != 0;
            ApplyPositionMask = (appliedBitMask & 0x10) != 0;
            ApplyTimeMask = (appliedBitMask & 0x20) != 0;
            ApplyStaticHold = (appliedBitMask & 0x40) != 0;
            ApplyDGPS = (appliedBitMask & 0x80) != 0;
            ApplyCnoThreshold = (appliedBitMask & 0x100) != 0;
            ApplyUTC = (appliedBitMask & 0x400) != 0;

            PlatformModel = (ModelEnum)buffer[byteIndex++];
            PositionMode = (PositionModeEnum)buffer[byteIndex++];
            FixedAltitude = BitConverter.ToInt32(buffer, (int)byteIndex) * 0.01; byteIndex += 4;
            FixedAltitudeVariance = BitConverter.ToUInt32(buffer, (int)byteIndex) * 0.0001; byteIndex += 4;
            MinimumElevation = (sbyte)buffer[byteIndex++];
            DrLimit = buffer[byteIndex++];
            PositionDOP = BitConverter.ToUInt16(buffer, (int)byteIndex) * 0.1; byteIndex += 2;
            TimeDOP = BitConverter.ToUInt16(buffer, (int)byteIndex) * 0.1; byteIndex += 2;
            PositionAccuracy = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            TimeAccuracy = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            StaticHoldThreshold = buffer[byteIndex++] * 0.01;
            DGNSSTimeout = buffer[byteIndex++];
            CnoThreshNumSVs = buffer[byteIndex++];
            CnoThreshold = buffer[byteIndex]; byteIndex += 3;
            StaticHoldMaxDistance = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            UtcStandard = buffer[byteIndex]; byteIndex += 6;


            return byteIndex * 8 - offsetBits;
        }
    }

}