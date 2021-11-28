using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace Asv.Gnss
{
    public class AsvServerConfig
    {
        public int HeartBeatSendIntervalMs { get; set; } = 1000;
        public AsvDeviceType DeviceType { get; set; } = AsvDeviceType.Unknown;
        public byte SenderId { get; set; } = 255;
        public int SendTimeoutMs { get; set; } = 1000;
        public int HeartBeatListenTimeMs { get; set; } = 2000;
    }

    public class AsvServer:IDisposable
    {
        private readonly GnssConnection _conn;
        private readonly IDiagnosticSource _diag;
        private readonly AsvServerConfig _config;
        private readonly CancellationTokenSource _disposeCancel = new CancellationTokenSource();
        private readonly MessageSequenceCalculator _sequenceCalculator;
        private int _heartBeatIsBusy;
        private readonly SpeedIndicator _hbSpeed;
        private readonly LinkQualityCalculator _qualityCalculator;

        public AsvServer(GnssConnection conn, IDiagnosticSource diag, AsvServerConfig config)
        {
            _conn = conn;
            _diag = diag;
            _config = config;
            if (config.HeartBeatSendIntervalMs > 0)
            {
                Observable.Timer(TimeSpan.FromMilliseconds(config.HeartBeatSendIntervalMs),
                    TimeSpan.FromMilliseconds(config.HeartBeatSendIntervalMs)).Subscribe(SendHeartBeat, _disposeCancel.Token);
            }
            _sequenceCalculator = new MessageSequenceCalculator();
            _hbSpeed = _diag.Speed["HB TX"];

            _qualityCalculator = new LinkQualityCalculator(conn, config.HeartBeatListenTimeMs);
            _disposeCancel.Token.Register(() => _qualityCalculator.Dispose());

            RawHeartbeat.Subscribe(_ => _diag.Int["Send ID"] = _.SenderId,_disposeCancel.Token);
            PacketRateHz.Subscribe(_ => _diag.Int["RX rate"] = _, _disposeCancel.Token);
            LinkQuality.Subscribe(_ => _diag.Str["RX qual"] = _.ToString("P1"), _disposeCancel.Token);
            Link.Subscribe(_ => _diag.Str["RX state"] = _.ToString("G"), _disposeCancel.Token);
        }

        public IRxValue<AsvMessageHeartBeat> RawHeartbeat => _qualityCalculator.RawHeartbeat;
        public IRxValue<int> PacketRateHz => _qualityCalculator.PacketRateHz;
        public IRxValue<double> LinkQuality => _qualityCalculator.LinkQuality;
        public IRxValue<LinkState> Link => _qualityCalculator.Link;

        public AsvDeviceState DeviceState { get; set; } = AsvDeviceState.Unknown;

        private async void SendHeartBeat(long obj)
        {
            if (Interlocked.CompareExchange(ref _heartBeatIsBusy, 1, 0) != 0)
            {
                _diag.Int["HB skip"]++;
            }
            var msg = new AsvMessageHeartBeat
            {
                DeviceState = DeviceState,
                DeviceType = _config.DeviceType,
                SenderId = _config.SenderId,
                Sequence = _sequenceCalculator.GetNextSequenceNumber(),
                TargetId = 0,
            };
            using (var timeoutCancel = new CancellationTokenSource(_config.SendTimeoutMs))
            using (var linkCancel = CancellationTokenSource.CreateLinkedTokenSource(_disposeCancel.Token, timeoutCancel.Token))
            {
                try
                {
                    await _conn.Send(msg, linkCancel.Token);
                    _hbSpeed.Increment(1);
                }
                catch (Exception e)
                {
                    _diag.Int["HB err"]++;
                }    
            }

            Interlocked.Exchange(ref _heartBeatIsBusy, 0);
        }


        public void Dispose()
        {
            _disposeCancel.Cancel(false);
            _disposeCancel.Dispose();
        }
    }


    public class LinkQualityCalculator 
    {
        private readonly int _heartBeatTimeoutMs;
        private readonly RxValue<AsvMessageHeartBeat> _heartBeat = new RxValue<AsvMessageHeartBeat>();
        private readonly CancellationTokenSource _disposeCancel = new CancellationTokenSource();
        private readonly RxValue<int> _packetRate = new RxValue<int>();
        private readonly RxValue<double> _linkQuality = new RxValue<double>();
        private readonly LinkIndicator _link = new LinkIndicator(3);
        private DateTime _lastHeartbeat;
        private int _lastPacketId;
        private int _packetCounter;
        private int _prev;

        public LinkQualityCalculator(GnssConnection connection, int heartBeatTimeoutMs = 2000)
        {
            _heartBeatTimeoutMs = heartBeatTimeoutMs;
            connection.Filter<AsvMessageHeartBeat>()
                .Select(_ => _.Sequence)
                .Subscribe(_ =>
                {
                    Interlocked.Exchange(ref _lastPacketId, _);
                    Interlocked.Increment(ref _packetCounter);
                }, _disposeCancel.Token);


            connection.Filter<AsvMessageHeartBeat>()
                .Subscribe(_heartBeat);
            _disposeCancel.Token.Register(() => _heartBeat.Dispose());

            connection.Filter<AsvMessageHeartBeat>()
                .Select(_ => 1)
                .Buffer(TimeSpan.FromSeconds(1))
                .Select(_ => _.Sum()).Subscribe(_packetRate, _disposeCancel.Token);
            _disposeCancel.Token.Register(() => _packetRate.Dispose());

            Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Subscribe(CheckConnection, _disposeCancel.Token);
            RawHeartbeat.Subscribe(_ =>
            {
                if (_disposeCancel.IsCancellationRequested) return;
                _lastHeartbeat = DateTime.Now;
                _link.Upgrade();
                CalculateLinqQuality();
            }, _disposeCancel.Token);
            _disposeCancel.Token.Register(() => _link.Dispose());
        }

        private void CalculateLinqQuality()
        {
            if (_packetCounter <= 5) return;
            var last = _lastPacketId;
            var count = Interlocked.Exchange(ref _packetCounter, 0);
            var first = Interlocked.Exchange(ref _prev, last);

            var seq = last - first;
            if (seq < 0) seq = last + byte.MaxValue - first + 1;
            _linkQuality.OnNext(((double)count) / seq);
        }

        public IRxValue<AsvMessageHeartBeat> RawHeartbeat => _heartBeat;
        public IRxValue<int> PacketRateHz => _packetRate;
        public IRxValue<double> LinkQuality => _linkQuality;
        public IRxValue<LinkState> Link => _link;

        private void CheckConnection(long value)
        {
            if (DateTime.Now - _lastHeartbeat > TimeSpan.FromMilliseconds(_heartBeatTimeoutMs))
            {
                _link.Downgrade();
            }
        }

        public void Dispose()
        {
            _disposeCancel?.Cancel(false);
            _disposeCancel?.Dispose();
        }
    }

}