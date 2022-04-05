using System;
using Asv.Tools;

namespace Asv.Gnss
{
    public class UbxCfgTMode3 : UbxMessageBase
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

        public static byte[] SetMovingBaseStation()
        {
            var msg = new UbxCfgTMode3
            {
                Mode = ReceiverMode.Disabled,
                IsGivenInLLA = false
            };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        public static byte[] SetSurveyInBaseStation(uint minDuration = 300, double positionAccuracyLimit = 1.5)
        {
            var msg = new UbxCfgTMode3
            {
                Mode = ReceiverMode.SurveyIn,
                FixedPosition3DAccuracy = 0.0,
                SurveyInMinDuration = minDuration,
                SurveyInPositionAccuracyLimit = positionAccuracyLimit
            };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        public static byte[] SetFixedBaseStation(GeoPoint point, double position3DAccuracy = 0.0001)
        {
            var msg = new UbxCfgTMode3
            {
                Mode = ReceiverMode.FixedMode,
                IsGivenInLLA = true,
                FixedPosition3DAccuracy = position3DAccuracy,
                SurveyInPositionAccuracyLimit = 0.2,
                Location = point
            };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset;

            buffer[bitIndex++] = Version;
            buffer[bitIndex++] = 0;

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
            buffer[bitIndex++] = flags[0];
            buffer[bitIndex++] = flags[1];

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
                buffer[bitIndex++] = latBuff[0];
                buffer[bitIndex++] = latBuff[1];
                buffer[bitIndex++] = latBuff[2];
                buffer[bitIndex++] = latBuff[3];

                var lonBuff = BitConverter.GetBytes(lon);
                buffer[bitIndex++] = lonBuff[0];
                buffer[bitIndex++] = lonBuff[1];
                buffer[bitIndex++] = lonBuff[2];
                buffer[bitIndex++] = lonBuff[3];

                var altBuff = BitConverter.GetBytes(alt);
                buffer[bitIndex++] = altBuff[0];
                buffer[bitIndex++] = altBuff[1];
                buffer[bitIndex++] = altBuff[2];
                buffer[bitIndex++] = altBuff[3];

                buffer[bitIndex++] = latHp;
                buffer[bitIndex++] = lonHp;
                buffer[bitIndex] = altHp;

                bitIndex += 2;
            }
            else
            {
                for (var i = 0; i < 16; i++)
                {
                    buffer[bitIndex++] = 0;
                }
            }

            var fixedPosition3DAccuracy = BitConverter.GetBytes((uint)Math.Round(FixedPosition3DAccuracy * 10000.0));
            buffer[bitIndex++] = fixedPosition3DAccuracy[0];
            buffer[bitIndex++] = fixedPosition3DAccuracy[1];
            buffer[bitIndex++] = fixedPosition3DAccuracy[2];
            buffer[bitIndex++] = fixedPosition3DAccuracy[3];

            var surveyInMinDuration = BitConverter.GetBytes(SurveyInMinDuration);
            buffer[bitIndex++] = surveyInMinDuration[0];
            buffer[bitIndex++] = surveyInMinDuration[1];
            buffer[bitIndex++] = surveyInMinDuration[2];
            buffer[bitIndex++] = surveyInMinDuration[3];

            var surveyInPositionAccuracyLimit = BitConverter.GetBytes((uint)Math.Round(SurveyInPositionAccuracyLimit * 10000.0));
            buffer[bitIndex++] = surveyInPositionAccuracyLimit[0];
            buffer[bitIndex++] = surveyInPositionAccuracyLimit[1];
            buffer[bitIndex++] = surveyInPositionAccuracyLimit[2];
            buffer[bitIndex++] = surveyInPositionAccuracyLimit[3];
            bitIndex += 8;
            

            return bitIndex - offset;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            Version = buffer[bitIndex]; bitIndex += 2;
            var flags = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            Mode = (ReceiverMode)(flags & 0xFF);
            IsGivenInLLA = (flags & 0x100) != 0;

            if (Mode == ReceiverMode.FixedMode)
            {
                var ecefXorLat = BitConverter.ToInt32(buffer, (int)bitIndex); bitIndex += 4;
                var ecefYorLon = BitConverter.ToInt32(buffer, (int)bitIndex); bitIndex += 4;
                var ecefZorAlt = BitConverter.ToInt32(buffer, (int)bitIndex); bitIndex += 4;
                var ecefXOrLatHP = (sbyte)buffer[bitIndex++];
                var ecefYOrLonHP = (sbyte)buffer[bitIndex++];
                var ecefZOrAltHP = (sbyte)buffer[bitIndex];
                bitIndex += 2;

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
                bitIndex += 16;
            }

            FixedPosition3DAccuracy = BitConverter.ToUInt32(buffer, (int)bitIndex) * 0.0001; bitIndex += 4;
            SurveyInMinDuration = BitConverter.ToUInt32(buffer, (int)bitIndex); bitIndex += 4; ;
            SurveyInPositionAccuracyLimit = BitConverter.ToUInt32(buffer, (int)bitIndex) * 0.0001; bitIndex += 4 + 8;

            return bitIndex - offsetBits;
        }
    }
}