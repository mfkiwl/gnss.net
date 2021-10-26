using System;
using System.Runtime.Serialization;

namespace Asv.Gnss
{
    public interface IGnssParser:IDisposable
    {
        string ProtocolId { get; }
        bool Read(byte data);
        void Reset();
        IObservable<GnssParserException> OnError { get; }
        IObservable<GnssMessageBase> OnMessage { get; }
    }


    public static class ParserHelper
    {
        public static ComNavBinaryParser RegisterDefaultFrames(this ComNavBinaryParser src)
        {
            src.Register(() => new ComNavBinaryPsrPosPacket());
            return src;
        }

        public static RtcmV3Parser RegisterDefaultFrames(this RtcmV3Parser src)
        {
            src.Register(() => new RtcmV3MSM4(1074));
            src.Register(() => new RtcmV3MSM4(1084));
            src.Register(() => new RtcmV3MSM4(1094));
            src.Register(() => new RtcmV3MSM4(1124));
            src.Register(() => new RtcmV3MSM7(1077));
            src.Register(() => new RtcmV3MSM7(1087));
            src.Register(() => new RtcmV3MSM7(1097));
            src.Register(() => new RtcmV3MSM7(1127));
            src.Register(() => new RtcmV3Message1005());
            src.Register(() => new RtcmV3Message1006());
            src.Register(() => new RtcmV3Message1019());
            src.Register(() => new RtcmV3Message1020());
            return src;
        }

        public static RtcmV2Parser RegisterDefaultFrames(this RtcmV2Parser src)
        {
            src.Register(() => new RtcmV2Message1());
            src.Register(() => new RtcmV2Message31());
            src.Register(() => new RtcmV2Message17());
            return src;
        }

        public static Nmea0183Parser RegisterDefaultFrames(this Nmea0183Parser src)
        {
            src.Register(()=>new Nmea0183MessageGGA());
            src.Register(() => new Nmea0183MessageGLL());
            src.Register(() => new Nmea0183MessageGSA());
            src.Register(() => new Nmea0183MessageGST());
            src.Register(() => new Nmea0183MessageGSV());
            return src;
        }

        public static AsvParser RegisterDefaultFrames(this AsvParser src)
        {
            src.Register(() => new AsvMessageGbasVdbSend());
            src.Register(() => new AsvMessageHeartBeat());
            return src;
        }
    }


}