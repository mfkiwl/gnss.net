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
            return src;
        }

        public static Nmea0183Parser RegisterDefaultFrames(this Nmea0183Parser src)
        {
            src.Register(()=>new Nmea0183MessageGGA());
            return src;
        }
    }


}