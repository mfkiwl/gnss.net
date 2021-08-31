using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asv.Gnss
{
    public class RtcmV3Header: ISerializable
    {
        /// <summary>
        /// glonass\gps epoch time
        /// </summary>
        public uint Epoch { get; set; }
        /// <summary>
        /// message no
        /// </summary>
        public ushort MessageNumber { get; set; }
        /// <summary>
        /// no of satellites
        /// </summary>
        public byte NumberOfSat { get; set; }
        /// <summary>
        /// Reference station id 
        /// </summary>
        public ushort ReferenceStationId { get; set; }
        /// <summary>
        /// smoothing indicator
        /// </summary>
        public byte SmoothIndicator { get; set; }
        /// <summary>
        /// smoothing interval
        /// </summary>
        public byte SmoothInterval { get; set; }
        /// <summary>
        /// synchronous gnss flag
        /// </summary>
        public byte Sync { get; set; }

        public void Deserialize(byte[] buffer, uint startIndex = 0)
        {
            uint i = 24 + startIndex;

            MessageNumber = (ushort)BitOperationHelper.GetBitU(buffer, i, 12);
            i += 12; /* message no */
            ReferenceStationId = (ushort)BitOperationHelper.GetBitU(buffer, i, 12);
            i += 12; /* ref station id */
            if (MessageNumber < 1009 || MessageNumber > 1012)
            {
                Epoch = BitOperationHelper.GetBitU(buffer, i, 30);
                i += 30; /* gps epoch time */
            }
            else
            {
                Epoch = BitOperationHelper.GetBitU(buffer, i, 27);
                i += 27; /* glonass epoch time */
            }
            Sync = (byte)BitOperationHelper.GetBitU(buffer, i, 1);
            i += 1; /* synchronous gnss flag */
            NumberOfSat = (byte)BitOperationHelper.GetBitU(buffer, i, 5);
            i += 5; /* no of satellites */
            SmoothIndicator = (byte)BitOperationHelper.GetBitU(buffer, i, 1);
            i += 1; /* smoothing indicator */
            SmoothInterval = (byte)BitOperationHelper.GetBitU(buffer, i, 3);
            i += 3; /* smoothing interval */
        }

        public void Serialize(byte[] buffer, uint startIndex = 0)
        {
            uint i = 24;

            BitOperationHelper.SetBitU(buffer, i, 12, MessageNumber);
            i += 12; /* message no */
            BitOperationHelper.SetBitU(buffer, i, 12, ReferenceStationId);
            i += 12; /* ref station id */
            BitOperationHelper.SetBitU(buffer, i, 30, Epoch);
            i += 30; /* gps epoch time */
            BitOperationHelper.SetBitU(buffer, i, 1, Sync);
            i += 1; /* synchronous gnss flag */
            BitOperationHelper.SetBitU(buffer, i, 5, NumberOfSat);
            i += 5; /* no of satellites */
            BitOperationHelper.SetBitU(buffer, i, 1, SmoothIndicator);
            i += 1; /* smoothing indicator */
            BitOperationHelper.SetBitU(buffer, i, 3, SmoothInterval);
            i += 3; /* smoothing interval */
        }
    }
}
