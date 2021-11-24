using System;

namespace Asv.Gnss
{
    public class SbfPacketDOP:SbfPacketBase
    {
        public override ushort MessageRevision => 0;
        public override ushort MessageType => 4001;
        protected override void DeserializeMessage(byte[] buffer, uint offsetBits)
        {
            var offset = (int)(offsetBits / 8);
            NrSV = buffer[offset]; offset+=1;
            Reserved = buffer[offset]; offset +=1;
            PDOP = BitConverter.ToUInt16(buffer, offset)*0.01;offset+=2;
            TDOP = BitConverter.ToUInt16(buffer, offset) * 0.01; offset += 2;
            HDOP = BitConverter.ToUInt16(buffer, offset) * 0.01; offset += 2;
            VDOP = BitConverter.ToUInt16(buffer, offset) * 0.01; offset += 2;
            HPL = CheckNan(BitConverter.ToDouble(buffer, offset)); offset+=4;
            VPL = CheckNan(BitConverter.ToDouble(buffer, offset)); offset += 4;

        }
        /// <summary>
        /// Vertical Protection Level (see the DO 229 standard).
        /// </summary>
        public double VPL { get; set; }
        /// <summary>
        /// Horizontal Protection Level (see the DO 229 standard)
        /// </summary>
        public double HPL { get; set; }

        public double VDOP { get; set; }

        public double HDOP { get; set; }

        public double TDOP { get; set; }

        /// <summary>
        /// If 0, PDOP not available
        /// </summary>
        public double PDOP { get; set; }

        public byte Reserved { get; set; }

        /// <summary>
        /// Total number of satellites used in the DOP computation, or 0 if the DOP
        /// information is not available(in that case, the xDOP fields are all set to 0)
        /// </summary>
        public byte NrSV { get; set; }

        private static double CheckNan(double toSingle)
        {
            return toSingle == -2E10 ? Single.NaN : toSingle;
        }
    }
}