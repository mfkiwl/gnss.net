using System;

namespace Asv.Gnss
{
    public class UbxVelocitySolutionInNED : UbxMessageBase
    {
        public override byte Class => 0x01;
        public override byte SubClass => 0x12;

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 36;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (int)((offsetBits + base.Deserialize(buffer, offsetBits)) / 8);

            iTOW = BitConverter.ToUInt32(buffer, byteIndex); byteIndex += 4;
            NorthVelocity = BitConverter.ToInt32(buffer, byteIndex) * 0.01; byteIndex += 4;
            EastVelocity = BitConverter.ToInt32(buffer, byteIndex) * 0.01; byteIndex += 4;
            DownVelocity = BitConverter.ToInt32(buffer, byteIndex) * 0.01; byteIndex += 4;
            Speed3D = BitConverter.ToUInt32(buffer, byteIndex) * 0.01; byteIndex += 4;
            GroundSpeed2D = BitConverter.ToUInt32(buffer, byteIndex) * 0.01; byteIndex += 4;
            HeadingOfMotion2D = BitConverter.ToInt32(buffer, byteIndex) * 1e-5; byteIndex += 4;
            SpeedAccuracyEstimate = BitConverter.ToUInt32(buffer, byteIndex) * 0.01; byteIndex += 4;
            CourseAccuracyEstimate = BitConverter.ToUInt32(buffer, byteIndex) * 1e-5; byteIndex += 4;

            return (uint)(byteIndex * 8 - offsetBits);
        }

        public uint iTOW { get; set; }

        public double NorthVelocity { get; set; }

        public double EastVelocity { get; set; }

        public double DownVelocity { get; set; }

        public double Speed3D { get; set; }

        public double GroundSpeed2D { get; set; }

        public double HeadingOfMotion2D { get; set; }

        public double SpeedAccuracyEstimate { get; set; }

        public double CourseAccuracyEstimate { get; set; }
    }
}