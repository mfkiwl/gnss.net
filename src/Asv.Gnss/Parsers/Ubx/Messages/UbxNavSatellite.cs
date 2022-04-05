using System;
using Newtonsoft.Json;

namespace Asv.Gnss
{
    public class UbxNavSatellite : UbxMessageBase
    {
        public override byte Class => 0x01;
        public override byte SubClass => 0x35;

        public enum GnssId : byte
        {
            GPS = 0,
            SBAS = 1,
            Galileo = 2,
            BeiDou = 3,
            IMES = 4,
            QZSS = 5,
            GLONASS = 6,
        }

        public class UbxNavSatelliteItem
        {
            public GnssId GnssType { get; set; }
            /// <summary>
            /// GNSS identifier (see Satellite Numbering) for assignment
            /// </summary>
            public byte GnssId { get; set; }
            /// <summary>
            /// Satellite identifier (see Satellite Numbering) for assignment
            /// </summary>
            public byte SvId { get; set; }
            /// <summary>
            /// Carrier to noise ratio (signal strength)
            /// </summary>
            public byte CnobBHz { get; set; }
            /// <summary>
            /// Elevation (range: +/-90), unknown if out of range
            /// </summary>
            public sbyte ElevDeg { get; set; }
            /// <summary>
            /// Azimuth (range 0-360), unknown if elevation is out of range
            /// </summary>
            public short AzimDeg { get; set; }
            /// <summary>
            /// Pseudorange residual
            /// </summary>
            public double PrResM { get; set; }

            public uint Deserialize(byte[] buffer, uint offsetBits)
            {
                var bitIndex = offsetBits;

                GnssId = buffer[bitIndex++];
                GnssType = (GnssId)GnssId;
                SvId = buffer[bitIndex++];
                CnobBHz = buffer[bitIndex++];
                ElevDeg = (sbyte)buffer[bitIndex++];
                AzimDeg = BitConverter.ToInt16(buffer, (int)bitIndex); bitIndex += 2;
                PrResM = BitConverter.ToInt16(buffer, (int)bitIndex) * 0.1; bitIndex += 6;

                return bitIndex - offsetBits;
            }

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }
        /// <summary>
        /// GPS time of week of the navigation epoch. See the description of iTOW for details.
        /// </summary>
        public ulong iTOW { get; set; }
        /// <summary>
        /// Message version (0x01 for this version)
        /// </summary>
        public byte Version { get; set; }
        /// <summary>
        /// Number of satellites
        /// </summary>
        public byte NumSvs { get; set; }
        public UbxNavSatelliteItem[] Items { get; set; }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            iTOW = BitConverter.ToUInt64(buffer, (int)bitIndex); bitIndex += 4;
            Version = buffer[bitIndex++];
            NumSvs = buffer[bitIndex]; bitIndex += 3;
            Items = new UbxNavSatelliteItem[NumSvs];
            for (var i = 0; i < NumSvs; i++)
            {
                Items[i] = new UbxNavSatelliteItem();
                bitIndex += Items[i].Deserialize(buffer, bitIndex);
            }
            return bitIndex - offsetBits;
        }
    }
}