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
        public static RtcmV3Parser RegisterDefaultFrames(this RtcmV3Parser src)
        {
            src.Register(() => new RtcmV3MSM4(1074));
            src.Register(() => new RtcmV3MSM4(1084));
            src.Register(() => new RtcmV3MSM4(1094));
            src.Register(() => new RtcmV3MSM4(1124));
            return src;
        }

        public static Nmea0183Parser RegisterDefaultFrames(this Nmea0183Parser src)
        {
            src.Register(()=>new Nmea0183MessageGGA());
            return src;
        }
    }


}