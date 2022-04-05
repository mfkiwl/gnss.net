using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;
using NLog;

namespace Asv.Gnss
{
    public class UbxDevice : DisposableOnceWithCancel, IUbxDevice
    {
        private readonly GnssConnection _connection;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly TimeSpan CommandTimeoutMs = TimeSpan.FromSeconds(5);

        private readonly Subject<UbxMessageBase> _onUbxPacket = new Subject<UbxMessageBase>();
        private readonly Subject<RtcmV3MessageBase> _onRtcmPacket = new Subject<RtcmV3MessageBase>();
        
        private readonly Subject<UbxAck> _onUbxAckPacket = new Subject<UbxAck>();
        private readonly Subject<UbxNak> _onUbxNakPacket = new Subject<UbxNak>();

        private readonly RxValue<GeoPoint> _onMovingBasePosition = new RxValue<GeoPoint>();
        private readonly RxValue<UbxNavSurveyIn> _onSurveyIn = new RxValue<UbxNavSurveyIn>();
        private readonly RxValue<UbxNavPvt> _onUbxNavPvt = new RxValue<UbxNavPvt>();
        private readonly RxValue<UbxMonitorHardware> _onUbxMonHwPacket = new RxValue<UbxMonitorHardware>();
        private readonly RxValue<UbxMonitorVersion> _onUbxVersionPacket = new RxValue<UbxMonitorVersion>();
        private readonly RxValue<UbxCfgTMode3> _onUbxTMode3Packet = new RxValue<UbxCfgTMode3>();
        private readonly RxValue<UbxInfWarning> _onUbxInfWarningPacket = new RxValue<UbxInfWarning>();

        public UbxDevice(string connectionString, IDiagnosticSource diagnosticSource, params IGnssParser[] parsers) :
            this(new GnssConnection(connectionString, diagnosticSource, parsers), true)
        {
        }

        public UbxDevice(GnssConnection connection, bool disposeConnection)
        {
            _connection = connection;

            _connection.OnMessage.Where(_ => _.ProtocolId == UbxBinaryParser.GnssProtocolId)
                .Select(_ => _ as UbxMessageBase).Subscribe(__ => _onUbxPacket.OnNext(__), DisposeCancel);

            _connection.OnMessage.Where(_ => _.ProtocolId == RtcmV3Parser.GnssProtocolId)
                .Select(_ => _ as RtcmV3MessageBase).Subscribe(__ => _onRtcmPacket.OnNext(__), DisposeCancel);

            SubscribeUbx();
            SubscribeRtcm();

            if (disposeConnection) Disposable.Add(connection);
            Disposable.Add(_onUbxPacket);
            Disposable.Add(_onUbxAckPacket);
            Disposable.Add(_onUbxNakPacket);

            Disposable.Add(_onMovingBasePosition);
            Disposable.Add(_onSurveyIn);
            Disposable.Add(_onUbxNavPvt);
            Disposable.Add(_onUbxMonHwPacket);
            Disposable.Add(_onUbxVersionPacket);
        }

        public IObservable<RtcmV3MessageBase> OnRTCM => _onRtcmPacket;
        public IObservable<UbxMessageBase> OnUbx => _onUbxPacket;
        public IObservable<UbxAck> OnAck => _onUbxAckPacket;
        public IObservable<UbxNak> OnNak => _onUbxNakPacket;
        
        public IRxValue<GeoPoint> OnMovingBasePosition => _onMovingBasePosition;
        public IRxValue<UbxNavSurveyIn> OnSurveyIn => _onSurveyIn;
        public IRxValue<UbxCfgTMode3> OnUbxTMode3 => _onUbxTMode3Packet;
        public IRxValue<UbxInfWarning> UbxWarning => _onUbxInfWarningPacket;
        public IRxValue<UbxNavPvt> OnMovingBase => _onUbxNavPvt;
        public IRxValue<UbxMonitorHardware> OnHwInfo => _onUbxMonHwPacket;
        public IRxValue<UbxMonitorVersion> OnVersion => _onUbxVersionPacket;

        #region Input data processing

        private void SubscribeUbx()
        {
            _onUbxPacket.Where(_ => _.MessageId == 0x0501)
                .Select(_ => _ as UbxAck).Subscribe(_onUbxAckPacket, DisposeCancel);
            _onUbxPacket.Where(_ => _.MessageId == 0x0500)
                .Select(_ => _ as UbxNak).Subscribe(_onUbxNakPacket, DisposeCancel);



            _onUbxPacket.Where(_ => _.MessageId == 0x013b)
                .Select(_ => _ as UbxNavSurveyIn).Subscribe(_onSurveyIn, DisposeCancel);
            _onUbxPacket.Where(_ => _.MessageId == 0x0107)
                .Select(_ => _ as UbxNavPvt).Subscribe(_onUbxNavPvt, DisposeCancel);
            _onUbxPacket.Where(_ => _.MessageId == 0x0671).Select(_ => _ as UbxCfgTMode3)
                .Subscribe(_onUbxTMode3Packet, DisposeCancel);



            _onUbxPacket.Where(_ => _.MessageId == 0x0A04)
                .Select(_ => _ as UbxMonitorVersion).Subscribe(_onUbxVersionPacket, DisposeCancel);
            _onUbxPacket.Where(_ => _.MessageId == 0x0A09)
                .Select(_ => _ as UbxMonitorHardware).Subscribe(_onUbxMonHwPacket, DisposeCancel);
            _onUbxPacket.Where(_ => _.MessageId == 0x0401).Select(_ => _ as UbxInfWarning)
                .Subscribe(_onUbxInfWarningPacket, DisposeCancel);
        }

        private void SubscribeRtcm()
        {
            _onRtcmPacket.Where(_ => _.MessageId == 1005).Select(_ => _ as RtcmV3Message1005).Select(_ => new GeoPoint(_.Latitude, _.Longitude, _.Altitude)).Subscribe(_onMovingBasePosition, DisposeCancel);
            _onRtcmPacket.Where(_ => _.MessageId == 1006).Select(_ => _ as RtcmV3Message1006).Select(_ => new GeoPoint(_.Latitude, _.Longitude, _.Altitude)).Subscribe(_onMovingBasePosition, DisposeCancel);
        }

        #endregion

        #region RAW packets

        public async Task<TPacket> GetUbxPacket<TPacket>(CancellationToken cancel)
            where TPacket : UbxMessageBase, new()
        {

            using (var timeoutCancel = new CancellationTokenSource(CommandTimeoutMs))
            using (var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel, timeoutCancel.Token))
            {
                IDisposable subscribe = null;
                try
                {
                    var eve = new AsyncAutoResetEvent(false);
                    var result = new TPacket();
                    var msgType = result.MessageId;
                    subscribe = _onUbxPacket.FirstAsync(_ => _.MessageId == msgType).Subscribe(_ =>
                    {
                        result = _ as TPacket;
                        eve.Set();
                    });

                    var packet = result.GenerateRequest();
                    if (!await _connection.DataStream.Send(packet, packet.Length, linkedCancel.Token))
                        throw new Exception("Error to send data to port");
                    await eve.WaitAsync(linkedCancel.Token);
                    return result;
                }
                finally
                {
                    subscribe?.Dispose();
                }
            }
        }

        public Task<UbxNavPvt> GetUbxNavPvt(CancellationToken cancel = default)
        {
            return GetUbxPacket<UbxNavPvt>(cancel);
        }

        public Task<UbxMonitorVersion> GetUbxMonVer(CancellationToken cancel = default)
        {
            return GetUbxPacket<UbxMonitorVersion>(cancel);
        }

        public Task<UbxNavSatellite> GetUbxNavSat(CancellationToken cancel = default)
        {
            return GetUbxPacket<UbxNavSatellite>(cancel);
        }

        public Task<UbxCfgTMode3> GetUbxCfgTMode3(CancellationToken cancel = default)
        {
            return GetUbxPacket<UbxCfgTMode3>(cancel);
        }

        #endregion

        #region TMODE3

        public async Task StopSurveyIn(CancellationToken cancel)
        {
            _logger.Info($"Disable RTK base survey in mode");
            var packet = UbxCfgTMode3.SetMovingBaseStation();
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
        }

        public async Task SetTMode3SurveyIn(uint duration, double accLimit, CancellationToken cancel)
        {
            _logger.Info($"Setup RTK base survey in mode duration:{duration}, accLimit:{accLimit}");
            var packet = UbxCfgTMode3.SetSurveyInBaseStation(duration, accLimit);
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
        }

        public async Task SetTMode3FixedBase(GeoPoint location, double accLimit, CancellationToken cancel)
        {
            _logger.Info($"Setup RTK base TMode3 fixed position mode location:{location}, accLimit:{accLimit}");
            var packet = UbxCfgTMode3.SetFixedBaseStation(location, accLimit);
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
        }

        public async Task SetTMode3MovingBase(CancellationToken cancel)
        {
            _logger.Info($"Setup RTK base TMode3 moving base mode");
            var packet = UbxCfgTMode3.SetMovingBaseStation();
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
        }

        #endregion

        #region Config

        public async Task SetupByDefault(CancellationToken cancel)
        {
            await SetPort(cancel);

            await SetRateTo1Hz(cancel);
            await SetStationaryMode(false, cancel);
            await TurnOffNMEA(cancel);

            // mon-ver
            await PollMsg((byte)UbxHelper.ClassIDs.MON, 0x04, cancel);

            // surveyin msg - for feedback
            await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x3B, 1, cancel);
            // await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x3B, 1, cancel);

            // pvt msg - for feedback
            await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x07, 1, cancel);

            // cfg msg
            await SetMessageRate((byte)UbxHelper.ClassIDs.CFG, 0x71, 5, cancel);

            // 1005 - 5s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x05, 5, cancel);

            byte rate1 = 0;
            byte rate2 = 1;
            
            // 1074 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x4A, rate2, cancel);
            // 1077 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x4D, rate1, cancel);

            // 1084 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x54, rate2, cancel);
            // 1087 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x57, rate1, cancel);

            // 1094 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x5E, rate2, cancel);
            // 1097 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x61, rate1, cancel);

            // 1124 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x7C, rate2, cancel);
            // 1127 - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x7F, rate1, cancel);



            // 1230 - 5s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0xE6, 5, cancel);

            // NAV-VELNED - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x12, 1, cancel);

            // rxm-raw/rawx - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x15, 1, cancel);
            await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x10, 1, cancel);

            // rxm-sfrb/sfrb - 2s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x13, 2, cancel);
            await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x11, 2, cancel);

            // mon-hw - 2s
            await SetMessageRate((byte)UbxHelper.ClassIDs.MON, 0x09, 2, cancel);

            await Task.Delay(100, cancel);
        }

        private async Task SetPort(CancellationToken cancel)
        {
            // set rate to 1hz
            var packetUart = UbxPortConfiguration.SetUart();
            var packetUsb = UbxPortConfiguration.SetUsb();
            if (!await _connection.DataStream.Send(packetUart, packetUart.Length, cancel)) throw new Exception("Error to send data to port");
            if (!await _connection.DataStream.Send(packetUsb, packetUsb.Length, cancel)) throw new Exception("Error to send data to port");
            await Task.Delay(200, cancel);
        }

        private async Task SetRateTo1Hz(CancellationToken cancel)
        {
            // set rate to 1hz
            var packet = UbxRateSettings.SetRate(1.0);
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
            await Task.Delay(200, cancel);
        }

        public async Task SetStationaryMode(bool movingBase, CancellationToken cancel)
        {
            if (!movingBase)
            {
                var packet = UbxNavigationEngineSettings.SetStationaryModel();
                if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
                await Task.Delay(200, cancel);
                // 4072
                await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0xFE, 0, cancel);
            }
            else
            {
                // 4072
                await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0xFE, 1, cancel);
            }
        }

        public async Task TurnOffNMEA(CancellationToken cancel)
        {
            // turn off all nmea
            for (int a = 0; a <= 0xf; a++)
            {
                if (a == 0x0B || a == 0x0C || a == 0x0E)
                    continue;
                await SetMessageRate((byte)UbxHelper.ClassIDs.NMEA, (byte)a, 0, cancel);
            }
        }

        public async Task SetMessageRate(byte msgClass, byte msgSubClass, byte msgRate, CancellationToken cancel)
        {
            var packet = UbxHelper.MsgTurnOn(msgClass, msgSubClass, msgRate);
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
            await Task.Delay(10, cancel);
        }

        public async Task PollMsg(byte msgClass, byte msgSubClass, CancellationToken cancel)
        {
            var packet = UbxHelper.GenerateRequest(msgClass, msgSubClass);
            if (!await _connection.DataStream.Send(packet, packet.Length, cancel)) throw new Exception("Error to send data to port");
            await Task.Delay(10, cancel);
        }

        #endregion

        
    }
}