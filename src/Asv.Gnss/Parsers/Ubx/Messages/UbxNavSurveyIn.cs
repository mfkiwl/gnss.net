using System;
using Asv.Tools;

namespace Asv.Gnss
{
    public class UbxNavSurveyIn : UbxMessageBase
    {
        public override byte Class => 0x01;
        public override byte SubClass => 0x3B;

        public byte Version { get; set; }
        public uint ITow { get; set; }
        public uint Duration { get; set; }
        public int MeanX { get; set; }
        public int MeanY { get; set; }
        public int MeanZ { get; set; }
        public (double X, double Y, double Z) Ecef { get; set; }
        public sbyte MeanXhp { get; set; }
        public sbyte MeanYhp { get; set; }
        public sbyte MeanZhp { get; set; }
        public double Accuracy { get; set; }
        public uint Observations { get; set; }
        public bool Valid { get; set; }
        public bool Active { get; set; }

        public GeoPoint? Location { get; set; }


        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            Version = buffer[byteIndex]; byteIndex += 4;
            ITow = BitConverter.ToUInt32(buffer, (int)byteIndex); byteIndex += 4;
            Duration = BitConverter.ToUInt32(buffer, (int)byteIndex); byteIndex += 4;
            MeanX = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            MeanY = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            MeanZ = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
            MeanXhp = (sbyte)buffer[byteIndex++];
            MeanYhp = (sbyte)buffer[byteIndex++];
            MeanZhp = (sbyte)buffer[byteIndex]; byteIndex += 2;
            Ecef = (X: MeanX * 0.01 + MeanXhp * 0.0001, Y: MeanY * 0.01 + MeanYhp * 0.0001,
                Z: MeanZ * 0.01 + MeanZhp * 0.0001);
            Accuracy = BitConverter.ToUInt32(buffer, (int)byteIndex) / 10000.0; byteIndex += 4;
            Observations = BitConverter.ToUInt32(buffer, (int)byteIndex); byteIndex += 4;
            Valid = buffer[byteIndex++] != 0;
            Active = buffer[byteIndex] != 0; byteIndex += 3;

            var position = UbxHelper.Ecef2Pos(Ecef);
            var lat = position.X * 180.0 / Math.PI;
            var lon = position.Y * 180.0 / Math.PI;
            var alt = position.Z;
            Location = new GeoPoint(lat, lon, alt);

            return byteIndex * 8 - offsetBits;
        }
    }
}