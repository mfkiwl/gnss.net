using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using System.Threading;

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

    public interface IGnssConnection:IDisposable
    {
        IObservable<GnssParserException> OnError { get; }
        IObservable<GnssMessageBase> OnMessage { get; }
    }

    public class GnssConnection : IGnssConnection
    {
        private readonly IGnssParser[] _parsers;
        private readonly CancellationTokenSource _disposeCancel = new CancellationTokenSource();
        private readonly object _sync = new object();
        private readonly Subject<GnssParserException> _onErrorSubject = new Subject<GnssParserException>();
        private readonly Subject<GnssMessageBase> _onMessageSubject = new Subject<GnssMessageBase>();
        private readonly bool _disposeDataStream;

        public GnssConnection(string connectionString, params IGnssParser[] parsers) : this(ConnectionStringConvert(connectionString), parsers)
        {
            _disposeDataStream = true;
        }

        private static IDataStream ConnectionStringConvert(string connString)
        {
            var p = PortFactory.Create(connString);
            p.Enable();
            return p;
        }

        public GnssConnection(IDataStream stream, params IGnssParser[] parsers)
        {
            DataStream = stream;
            _parsers = parsers;
            foreach (var parser in parsers)
            {
                parser.OnError.Subscribe(_onErrorSubject, _disposeCancel.Token);
                parser.OnMessage.Subscribe(_onMessageSubject, _disposeCancel.Token);
            }
            DataStream.SelectMany(_ => _).Subscribe(OnByteRecv, _disposeCancel.Token);
        }

        private void OnByteRecv(byte data)
        {
            lock (_sync)
            {
                var packetFound = false;
                foreach (var parser in _parsers)
                {
                    if (parser.Read(data))
                    {
                        packetFound = true;
                        break;
                    }
                }

                if (packetFound)
                {
                    foreach (var parser in _parsers)
                    {
                        parser.Reset();
                    }
                }
            }
        }

        public IDataStream DataStream { get; }

        public IObservable<GnssParserException> OnError => _onErrorSubject;
        public IObservable<GnssMessageBase> OnMessage => _onMessageSubject;

        public void Dispose()
        {
            _disposeCancel.Cancel(false);
            _disposeCancel.Dispose();
            _onErrorSubject?.Dispose();
            _onMessageSubject?.Dispose();
            if (_disposeDataStream)
            {
                (DataStream as IDisposable)?.Dispose();
            }
        }
    }



    public static class ParserHelper
    {
        public static RtcmV3Parser RegisterDefaultFrames(this RtcmV3Parser src)
        {
            return src;
        }

        public static Nmea0183Parser RegisterDefaultFrames(this Nmea0183Parser src)
        {
            src.Register(()=>new Nmea0183MessageGGA());
            return src;
        }
    }


}