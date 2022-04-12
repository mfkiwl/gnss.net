using System;
using Asv.Tools;

namespace Asv.Gnss
{
    public class UbxTimeModeConfiguration : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x71;

        public byte Version;
        public enum ReceiverMode : byte
        {
            Disabled = 0,
            SurveyIn = 1,
            FixedMode = 2,
            Reserved
        }
        public ReceiverMode Mode { get; set; }
        public bool IsGivenInLLA { get; set; }
        
        public double FixedPosition3DAccuracy;
        public uint SurveyInMinDuration = 60;
        public double SurveyInPositionAccuracyLimit;
        
        public GeoPoint? Location { get; set; }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 40;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            buffer[byteIndex++] = Version;
            buffer[byteIndex++] = 0;

            if (!Location.HasValue || Location.Value.Equals(GeoPoint.Zero))
            {
                if (Mode == ReceiverMode.FixedMode)
                {
                    Mode = ReceiverMode.SurveyIn;
                    IsGivenInLLA = false;
                }
            }
            else
            {
                if (Mode == ReceiverMode.FixedMode)
                    IsGivenInLLA = true;
            }
            
            var flags = BitConverter.GetBytes((ushort)((ushort)Mode | ((IsGivenInLLA ? 1 : 0) << 8)));
            buffer[byteIndex++] = flags[0];
            buffer[byteIndex++] = flags[1];

            if (Mode == ReceiverMode.FixedMode)
            {
                // ReSharper disable once PossibleInvalidOperationException
                var lat = (int)Math.Round(Location.Value.Latitude * 1e7);
                var lon = (int)Math.Round(Location.Value.Longitude * 1e7);
                var alt = (int)Math.Round((Location.Value.Altitude ?? 0) * 100.0);
                var xpX = (long)Math.Round(Location.Value.Latitude * 1e9);
                var xpY = (long)Math.Round(Location.Value.Longitude * 1e9);
                var xpZ = (long)Math.Round((Location.Value.Altitude ?? 0) * 10000.0);
                var latHp = (byte)(xpX - (long)lat * 100);
                var lonHp = (byte)(xpY - (long)lon * 100);
                var altHp = (byte)(xpZ - (long)alt * 100);

                var latBuff = BitConverter.GetBytes(lat);
                buffer[byteIndex++] = latBuff[0];
                buffer[byteIndex++] = latBuff[1];
                buffer[byteIndex++] = latBuff[2];
                buffer[byteIndex++] = latBuff[3];

                var lonBuff = BitConverter.GetBytes(lon);
                buffer[byteIndex++] = lonBuff[0];
                buffer[byteIndex++] = lonBuff[1];
                buffer[byteIndex++] = lonBuff[2];
                buffer[byteIndex++] = lonBuff[3];

                var altBuff = BitConverter.GetBytes(alt);
                buffer[byteIndex++] = altBuff[0];
                buffer[byteIndex++] = altBuff[1];
                buffer[byteIndex++] = altBuff[2];
                buffer[byteIndex++] = altBuff[3];

                buffer[byteIndex++] = latHp;
                buffer[byteIndex++] = lonHp;
                buffer[byteIndex] = altHp;

                byteIndex += 2;
            }
            else
            {
                for (var i = 0; i < 16; i++)
                {
                    buffer[byteIndex++] = 0;
                }
            }

            var fixedPosition3DAccuracy = BitConverter.GetBytes((uint)Math.Round(FixedPosition3DAccuracy * 10000.0));
            buffer[byteIndex++] = fixedPosition3DAccuracy[0];
            buffer[byteIndex++] = fixedPosition3DAccuracy[1];
            buffer[byteIndex++] = fixedPosition3DAccuracy[2];
            buffer[byteIndex++] = fixedPosition3DAccuracy[3];

            var surveyInMinDuration = BitConverter.GetBytes(SurveyInMinDuration);
            buffer[byteIndex++] = surveyInMinDuration[0];
            buffer[byteIndex++] = surveyInMinDuration[1];
            buffer[byteIndex++] = surveyInMinDuration[2];
            buffer[byteIndex++] = surveyInMinDuration[3];

            var surveyInPositionAccuracyLimit = BitConverter.GetBytes((uint)Math.Round(SurveyInPositionAccuracyLimit * 10000.0));
            buffer[byteIndex++] = surveyInPositionAccuracyLimit[0];
            buffer[byteIndex++] = surveyInPositionAccuracyLimit[1];
            buffer[byteIndex++] = surveyInPositionAccuracyLimit[2];
            buffer[byteIndex++] = surveyInPositionAccuracyLimit[3];
            byteIndex += 8;
            

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            Version = buffer[byteIndex]; byteIndex += 2;
            var flags = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            Mode = (ReceiverMode)(flags & 0xFF);
            IsGivenInLLA = (flags & 0x100) != 0;

            if (Mode == ReceiverMode.FixedMode)
            {
                var ecefXorLat = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
                var ecefYorLon = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
                var ecefZorAlt = BitConverter.ToInt32(buffer, (int)byteIndex); byteIndex += 4;
                var ecefXOrLatHP = (sbyte)buffer[byteIndex++];
                var ecefYOrLonHP = (sbyte)buffer[byteIndex++];
                var ecefZOrAltHP = (sbyte)buffer[byteIndex];
                byteIndex += 2;

                if (!IsGivenInLLA)
                {
                    (double X, double Y, double Z) ecef;
                    ecef.X = ecefXorLat * 0.01 + ecefXOrLatHP * 0.0001;
                    ecef.Y = ecefYorLon * 0.01 + ecefYOrLonHP * 0.0001;
                    ecef.Z = ecefZorAlt * 0.01 + ecefZOrAltHP * 0.0001;

                    var position = UbxHelper.Ecef2Pos(ecef);
                    var lat = position.X * 180.0 / Math.PI;
                    var lon = position.Y * 180.0 / Math.PI;
                    var alt = position.Z;
                    Location = new GeoPoint(lat, lon, alt);
                }
                else
                {
                    var lat = ecefXorLat * 1e-7 + ecefXOrLatHP * 1e-9;
                    var lon = ecefYorLon * 1e-7 + ecefYOrLonHP * 1e-9;
                    var alt = ecefZorAlt * 0.01 + ecefZOrAltHP * 0.0001;

                    Location = new GeoPoint(lat, lon, alt);
                }
            }
            else
            {
                byteIndex += 16;
            }

            FixedPosition3DAccuracy = BitConverter.ToUInt32(buffer, (int)byteIndex) * 0.0001; byteIndex += 4;
            SurveyInMinDuration = BitConverter.ToUInt32(buffer, (int)byteIndex); byteIndex += 4; ;
            SurveyInPositionAccuracyLimit = BitConverter.ToUInt32(buffer, (int)byteIndex) * 0.0001; byteIndex += 4 + 8;

            return byteIndex * 8 - offsetBits;
        }
    }

    public class UbxMovingBaseStation : UbxTimeModeConfiguration
    {
        public UbxMovingBaseStation()
        {
            Mode = ReceiverMode.Disabled;
            IsGivenInLLA = false;
        }
    }

    public class UbxSurveyInBaseStation : UbxTimeModeConfiguration
    {
        public UbxSurveyInBaseStation(uint minDuration = 60, double positionAccuracyLimit = 10)
        {
            Mode = ReceiverMode.SurveyIn;
            FixedPosition3DAccuracy = 0.0;
            SurveyInMinDuration = minDuration;
            SurveyInPositionAccuracyLimit = positionAccuracyLimit;
        }
    }

    public class UbxFixedBaseStation : UbxTimeModeConfiguration
    {
        public UbxFixedBaseStation(GeoPoint point, double position3DAccuracy = 0.0001)
        {
            Mode = ReceiverMode.FixedMode;
            IsGivenInLLA = true;
            FixedPosition3DAccuracy = position3DAccuracy;
            SurveyInPositionAccuracyLimit = 0.2;
            Location = point;
        }
    }
}