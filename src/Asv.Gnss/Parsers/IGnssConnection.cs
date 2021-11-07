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

        public static IObservable<TMsg> FilterWithTag<TMsg>(this IGnssConnection src, Action<TMsg> setTagCallback)
        {
            return src.OnMessage.Where(_ => _ is TMsg).Cast<TMsg>().Do(setTagCallback);
        }
        public static IObservable<TMsg> FilterWithTag<TMsg>(this IGnssConnection src, object tag)
            where TMsg: GnssMessageBase
        {
            return src.OnMessage.Where(_ => _ is TMsg).Cast<TMsg>().Do(_=>_.Tag = tag);
        }

        public static IObservable<TMsg> Filter<TMsg>(this IGnssConnection src, Func<TMsg, bool> filter)
        {
            return src.OnMessage.Where(_ => _ is TMsg).Cast<TMsg>().Where(filter);
        }
    }
}