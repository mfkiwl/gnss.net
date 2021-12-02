using System;

namespace Asv.Gnss
{
    public abstract class RtcmV2MessageBase : GnssMessageBaseWithId<ushort>
    {
        protected virtual DateTime adjhour(double zcnt)
        {
            var utc = DateTime.UtcNow;
            double tow = 0;
            var week = 0;
            
            /* if no time, get cpu time */
            var time = RtcmV3Helper.Utc2Gps(utc);
            
            RtcmV3Helper.GetFromTime(time, ref week, ref tow);
            
            var hour = Math.Floor(tow / 3600.0);
            var sec = tow - hour * 3600.0;
            if (zcnt < sec - 1800.0) zcnt += 3600.0;
            else if (zcnt > sec + 1800.0) zcnt -= 3600.0;
            
            return RtcmV3Helper.Gps2Utc(RtcmV3Helper.GetFromGps(week, hour * 3600 + zcnt));
        }

        private double GetUdre(byte rsHealth)
        {
            switch (rsHealth)
            {
                case 0:
                    return 1.0;
                case 1:
                    return 0.75;
                case 2:
                    return 0.5;
                case 3:
                    return 0.3;
                case 4:
                    return 0.2;
                case 5:
                    return 0.1;
                case 6:
                    return double.NaN;
                case 7:
                    return 0.0;
                default:
                    return double.NaN;
            }
        }

        private const byte SyncByte = 0x66;

        public override string ProtocolId => RtcmV2Parser.GnssProtocolId;

        public override int GetMaxByteSize()
        {
            return (2 /* HEADER */ + 31 /* MAX DATA STRING */) * 3 /* bytes each */;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits = 0)
        {
            throw new NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits = 0)
        {
            var bitIndex = offsetBits;

            var preamble = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;
            if (preamble != SyncByte)
            {
                throw new Exception($"Deserialization RTCMv2 message failed: want {SyncByte:X}. Read {preamble:X}");
            }

            var msgType = RtcmV3Helper.GetBitU(buffer, bitIndex, 6); bitIndex += 6;
            if (msgType != MessageId)
            {
                throw new Exception($"Deserialization RTCMv2 message failed: want message number '{MessageId}'. Read = '{msgType}'");
            }

            ReferenceStationId = (ushort)RtcmV3Helper.GetBitU(buffer, bitIndex, 10); bitIndex += 10;

            var zCountRaw = RtcmV3Helper.GetBitU(buffer, bitIndex, 13); bitIndex += 13;

            ZCount = zCountRaw * 0.6;

            if (ZCount >= 3600.0)
            {
                throw new Exception($"RTCMv2 Modified Z-count error: zcnt={ZCount}");
            }
            Gps = adjhour(ZCount);
            
            SequenceNumber = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 3); bitIndex += 3;
            // if (SequenceNumber - rtcm->seqno != 1 && SequenceNumber - rtcm->seqno != -7)
            // {
            //     trace(2, "rtcm2 message outage: seqno=%d->%d\n", rtcm->seqno, SequenceNumber);
            // }


            PayloadLength = (byte)(RtcmV3Helper.GetBitU(buffer, bitIndex, 5) * 3); bitIndex += 5;
            if (PayloadLength > (buffer.Length - 6 /* header 48 bit*/))
            {
                throw new Exception($"Deserialization RTCMv2 message failed: length too small. Want '{PayloadLength}'. Read = '{buffer.Length - 6}'");
            }

            Udre = GetUdre((byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 3)); bitIndex += 3;
            
            return bitIndex - offsetBits;
        }

        public double Udre { get; set; }

        public byte PayloadLength { get; set; }

        public byte SequenceNumber { get; set; }

        public DateTime Gps { get; set; }

        public double ZCount { get; set; }

        //public uint ZCountRaw { get; set; }

        public ushort ReferenceStationId { get; set; }
        
    }

    public class DObservationItem : ISerializable
    {
        private readonly NavigationSystemEnum _system;

        private DateTime GetDateTime(uint tb)
        {
            var utc = DateTime.UtcNow;
            var week = 0;
            var tow = 0.0;
            RtcmV3Helper.GetFromTime(utc, ref week, ref tow);
            var toe = (double)tb; /* lt->utc */
            var toh = tow % 3600.0;
            tow -= toh;

            if (toe < toh - 1800.0) toe += 3600.0;
            else if (toe > toh + 1800.0) toe -= 3600.0;
            return RtcmV3Helper.GetFromGps(week, tow + toe).AddHours(3.0);
        }

        public DObservationItem(NavigationSystemEnum system)
        {
            _system = system;
        }

        public int GetMaxByteSize()
        {
            return 5;
        }

        public uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new NotImplementedException();
        }

        public uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits;

            var fact = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 1); bitIndex += 1;
            var udre = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 2); bitIndex += 2;
            var prn = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 5); bitIndex += 5;
            var prc = RtcmV3Helper.GetBits(buffer, bitIndex, 16); bitIndex += 16;
            var rrc = RtcmV3Helper.GetBits(buffer, bitIndex, 8); bitIndex += 8;
            var iod = (byte)RtcmV3Helper.GetBitU(buffer, bitIndex, 8); bitIndex += 8;

            if (prn == 0) prn = 32;

            Prn = prn;

            if (prc == -32_768 || rrc == -128)
            {
                Prc = double.NaN;
                Rrc = double.NaN;
            }
            else
            {
                Prc = prc * (fact == 1 ? 0.32 : 0.02);
                Rrc = rrc * (fact == 1 ? 0.032 : 0.002);
            }
            SatelliteId = RtcmV3Helper.satno(_system, prn);
            Iod = _system == NavigationSystemEnum.SYS_GLO ? (byte)(iod & 0x7F) : iod;
            if (_system == NavigationSystemEnum.SYS_GLO)
                Tk = GetDateTime((uint) (Iod * 30));
            Udre = GetUdre(udre);

            return bitIndex - offsetBits;
        }

        private SatUdreEnum GetUdre(byte udre)
        {
            switch (udre)
            {
                case 0:
                    return SatUdreEnum.LessOne;
                case 1:
                    return SatUdreEnum.BetweenOneAndFour;
                case 2:
                    return SatUdreEnum.BetweenFourAndEight;
                case 3:
                    return SatUdreEnum.MoreEight;
                default:
                    throw new ArgumentOutOfRangeException(nameof(udre));
            }
        }

        public int SatelliteId { get; set; }

        public byte Prn { get; set; }

        public double Prc { get; set; }

        public double Rrc { get; set; }

        public byte Iod { get; set; }

        public SatUdreEnum Udre { get; set; }

        public DateTime Tk { get; set; }
    }

    public enum SatUdreEnum
    {
        /// <summary>
        /// One-sigma differential error less or equal than 1 met
        /// </summary>
        LessOne = 0,

        /// <summary>
        /// One-sigma differential error from 1 to 4 met
        /// </summary>
        BetweenOneAndFour = 1,

        /// <summary>
        /// One-sigma differential error from 4 to 8 met
        /// </summary>
        BetweenFourAndEight = 2,

        /// <summary>
        /// One-sigma differential error more than 8 met
        /// </summary>
        MoreEight = 3
    }
}