using System;
using System.Reactive.Linq;
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

    public static class GnssConnectionHelper
    {
        public static IObservable<TMsg> Filter<TMsg>(this IGnssConnection src)
        {
            return src.OnMessage.Where(_ => _ is TMsg).Cast<TMsg>();
        }

        public static IObservable<TMsg> Filter<TMsg>(this IGnssConnection src, Func<TMsg, bool> filter)
        {
            return src.OnMessage.Where(_ => _ is TMsg).Cast<TMsg>().Where(filter);
        }
    }
}