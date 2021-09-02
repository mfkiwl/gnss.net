using System;

namespace Asv.Gnss
{
    public interface IGnssConnection:IDisposable
    {
        IObservable<GnssParserException> OnError { get; }
        IObservable<GnssMessageBase> OnMessage { get; }
    }
}