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

        public static byte[] SetStationaryModel()
        {
            var msg = new UbxNavigationEngineSettings
            {
                PlatformModel = ModelEnum.Stationary,
                PositionMode = PositionModeEnum.Auto
            };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset;

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
            buffer[bitIndex++] = appliedBitMaskBuff[0];
            buffer[bitIndex++] = appliedBitMaskBuff[1];

            buffer[bitIndex++] = (byte)PlatformModel;
            buffer[bitIndex++] = (byte)PositionMode;

            var fixedAltitude = BitConverter.GetBytes((int)Math.Round(FixedAltitude * 100));
            buffer[bitIndex++] = fixedAltitude[0];
            buffer[bitIndex++] = fixedAltitude[1];
            buffer[bitIndex++] = fixedAltitude[2];
            buffer[bitIndex++] = fixedAltitude[3];

            var fixedAltitudeVariance = BitConverter.GetBytes((uint)Math.Round(FixedAltitudeVariance * 10000));
            buffer[bitIndex++] = fixedAltitudeVariance[0];
            buffer[bitIndex++] = fixedAltitudeVariance[1];
            buffer[bitIndex++] = fixedAltitudeVariance[2];
            buffer[bitIndex++] = fixedAltitudeVariance[3];

            buffer[bitIndex++] = (byte)MinimumElevation;
            buffer[bitIndex++] = DrLimit;

            var positionDOP = BitConverter.GetBytes((ushort)Math.Round(PositionDOP * 10));
            buffer[bitIndex++] = positionDOP[0];
            buffer[bitIndex++] = positionDOP[1];

            var timeDOP = BitConverter.GetBytes((ushort)Math.Round(TimeDOP * 10));
            buffer[bitIndex++] = timeDOP[0];
            buffer[bitIndex++] = timeDOP[1];

            var positionAccuracy = BitConverter.GetBytes(PositionAccuracy);
            buffer[bitIndex++] = positionAccuracy[0];
            buffer[bitIndex++] = positionAccuracy[1];

            var timeAccuracy = BitConverter.GetBytes(TimeAccuracy);
            buffer[bitIndex++] = timeAccuracy[0];
            buffer[bitIndex++] = timeAccuracy[1];

            buffer[bitIndex++] = (byte)Math.Round(StaticHoldThreshold * 100);
            buffer[bitIndex++] = DGNSSTimeout;
            buffer[bitIndex++] = CnoThreshNumSVs;
            buffer[bitIndex] = CnoThreshold; bitIndex += 3;

            var staticHoldMaxDistance = BitConverter.GetBytes(StaticHoldMaxDistance);
            buffer[bitIndex++] = staticHoldMaxDistance[0];
            buffer[bitIndex++] = staticHoldMaxDistance[1];

            buffer[bitIndex] = UtcStandard; bitIndex += 6;

            return bitIndex - offset;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            var appliedBitMask = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
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

            PlatformModel = (ModelEnum)buffer[bitIndex++];
            PositionMode = (PositionModeEnum)buffer[bitIndex++];
            FixedAltitude = BitConverter.ToInt32(buffer, (int)bitIndex) * 0.01; bitIndex += 4;
            FixedAltitudeVariance = BitConverter.ToUInt32(buffer, (int)bitIndex) * 0.0001; bitIndex += 4;
            MinimumElevation = (sbyte)buffer[bitIndex++];
            DrLimit = buffer[bitIndex++];
            PositionDOP = BitConverter.ToUInt16(buffer, (int)bitIndex) * 0.1; bitIndex += 2;
            TimeDOP = BitConverter.ToUInt16(buffer, (int)bitIndex) * 0.1; bitIndex += 2;
            PositionAccuracy = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            TimeAccuracy = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            StaticHoldThreshold = buffer[bitIndex++] * 0.01;
            DGNSSTimeout = buffer[bitIndex++];
            CnoThreshNumSVs = buffer[bitIndex++];
            CnoThreshold = buffer[bitIndex]; bitIndex += 3;
            StaticHoldMaxDistance = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            UtcStandard = buffer[bitIndex]; bitIndex += 6;


            return bitIndex - offsetBits;
        }
    }
}