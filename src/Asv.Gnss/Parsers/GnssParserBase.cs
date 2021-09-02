using System;
using System.Reactive.Subjects;

namespace Asv.Gnss
{
    public abstract class GnssParserBase : IGnssParser
    {
        private readonly Subject<GnssParserException> _onErrorSubject = new Subject<GnssParserException>();
        private readonly Subject<GnssMessageBase> _onMessageSubject = new Subject<GnssMessageBase>();
 
        public abstract string ProtocolId { get; }

        public abstract bool Read(byte data);
        public abstract void Reset();
       
        protected void InternalOnError(GnssParserException ex)
        {
            _onErrorSubject.OnNext(ex);
        }

        protected void InternalOnMessage(GnssMessageBase message)
        {
            _onMessageSubject.OnNext(message);
        }

        public IObservable<GnssParserException> OnError => _onErrorSubject;
        public IObservable<GnssMessageBase> OnMessage => _onMessageSubject;

        public void Dispose()
        {
            _onErrorSubject?.Dispose();
            _onMessageSubject?.Dispose();
        }
    }
}