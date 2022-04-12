using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Asv.Gnss
{
    public class PropertyObservable<TPacket> : DisposableOnceWithCancel, IObservable<TPacket>
    {
        private readonly Func<CancellationToken, Task<TPacket>> _propertyGetter;
        private readonly Subject<TPacket> _property = new();
        private readonly IObservable<long> _timer;
        private readonly int _commandTimeoutMs;
        private int _requestNotComplete;


        public PropertyObservable(Func<CancellationToken, Task<TPacket>> propertyGetter, TimeSpan dueTime, TimeSpan period)
        {
            _commandTimeoutMs = (int)Math.Floor(period.TotalMilliseconds);
            _propertyGetter = propertyGetter;
            _timer = Observable.Timer(dueTime, period).Do(UpdateProperty).Publish().RefCount();
            Disposable.Add(_property);
        }

        private async void UpdateProperty(long obj)
        {
            if (Interlocked.CompareExchange(ref _requestNotComplete, 1, 0) != 0) return;
            try
            {
                using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(DisposeCancel);
                linkedCancel.CancelAfter(_commandTimeoutMs);
                var value = await _propertyGetter(linkedCancel.Token).ConfigureAwait(false);
                _property.OnNext(value);
            }
            catch (TaskCanceledException)
            {
                if (!DisposeCancel.IsCancellationRequested)
                    _property.OnError(new TimeoutException(string.Format(
                        "Timeout to update property '{0}'. Timeout by {1:g} ms)", typeof(TPacket).Name, _commandTimeoutMs)));
            }
            catch (Exception e)
            {
                _property.OnError(e);
            }
            finally
            {
                Interlocked.Exchange(ref _requestNotComplete, 0);
            }
        }

        public IDisposable Subscribe(IObserver<TPacket> observer)
        {
            var propertySubscriber = _property.Subscribe(observer);
            var timerSubscriber = _timer.Subscribe();
            var subscriber = System.Reactive.Disposables.Disposable.Create(() =>
            {
                propertySubscriber.Dispose();
                timerSubscriber.Dispose();
            });
            Disposable.Add(subscriber);
            return subscriber;
        }

        protected override void InternalDisposeOnce()
        {
            _property.OnCompleted();
            base.InternalDisposeOnce();
        }
    }
}