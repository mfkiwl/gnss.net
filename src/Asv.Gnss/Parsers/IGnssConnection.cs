using System;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Gnss
{
    public interface IGnssConnection:IDisposable
    {
        IObservable<GnssParserException> OnError { get; }
        IObservable<GnssMessageBase> OnMessage { get; }
        Task Send(GnssMessageBase msg, CancellationToken cancel);
    }
}