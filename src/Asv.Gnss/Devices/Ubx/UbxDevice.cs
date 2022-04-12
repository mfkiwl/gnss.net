using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IGnssConnection _connection;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly TimeSpan CommandTimeoutMs = TimeSpan.FromSeconds(1);
        private const byte AttemptCount = 3;
        
        private readonly Subject<UbxMessageBase> _onUbxPacket = new Subject<UbxMessageBase>();
        private readonly Subject<RtcmV3MessageBase> _onRtcmPacket = new Subject<RtcmV3MessageBase>();
        private readonly Subject<RawRtcmV3Message> _onRawRtcmPacket = new Subject<RawRtcmV3Message>();
        
        private readonly Subject<UbxAck> _onUbxAckPacket = new Subject<UbxAck>();
        private readonly Subject<UbxNak> _onUbxNakPacket = new Subject<UbxNak>();

        private readonly RxValue<GeoPoint> _onMovingBasePosition = new RxValue<GeoPoint>();
        private readonly RxValue<UbxNavSurveyIn> _onSurveyIn = new RxValue<UbxNavSurveyIn>();
        private readonly RxValue<UbxNavPvt> _onUbxNavPvt = new RxValue<UbxNavPvt>();
        private readonly RxValue<UbxVelocitySolutionInNED> _onVelocitySolution = new RxValue<UbxVelocitySolutionInNED>();
        private readonly RxValue<UbxMonitorHardware> _onUbxMonHwPacket = new RxValue<UbxMonitorHardware>();
        
        private readonly PropertyObservable<UbxMonitorVersion> _onUbxVersionPacket;
        private readonly PropertyObservable<UbxTimeModeConfiguration> _onUbxTMode3Packet;
        private readonly Subject<UbxInfWarning> _onUbxInfWarningPacket = new();
        private readonly RxValue<bool> _onReboot = new();
        private int _rebootNotComplete;
        private byte _msmMsgRate = 1;
        

        public UbxDevice(string connectionString, IDiagnosticSource diagnosticSource, params IGnssParser[] parsers) :
            this(new GnssConnection(connectionString, diagnosticSource, parsers), true)
        {
        }

        public UbxDevice(IGnssConnection connection, bool disposeConnection)
        {
            _connection = connection;

            _connection.OnMessage.Where(_ => _.ProtocolId == UbxBinaryParser.GnssProtocolId)
                .Select(_ => _ as UbxMessageBase).Subscribe(__ => _onUbxPacket.OnNext(__), DisposeCancel);

            _connection.OnMessage.Where(_ => _.ProtocolId == RtcmV3Parser.GnssProtocolId)
                .Select(_ => _ as RtcmV3MessageBase).Subscribe(__ => _onRtcmPacket.OnNext(__), DisposeCancel);

            var rtcmParser = _connection.Parsers.FirstOrDefault(_ => _.ProtocolId == RtcmV3Parser.GnssProtocolId);
            if (rtcmParser != null)
            {
                (rtcmParser as RtcmV3Parser)?.OnRawData.Where(_ =>
                        _.MessageId == 1005 || _.MessageId == 1006 || _.MessageId == 1074 || _.MessageId == 1077 ||
                        _.MessageId == 1084 || _.MessageId == 1087 || _.MessageId == 1094 || _.MessageId == 1097 ||
                        _.MessageId == 1124 || _.MessageId == 1127 || _.MessageId == 1230 || _.MessageId == 4072)
                    .Select(_ => _ as RawRtcmV3Message).Subscribe(_ => _onRawRtcmPacket.OnNext(_), DisposeCancel);
            }

            _onUbxVersionPacket = new PropertyObservable<UbxMonitorVersion>(GetUbxMonVer, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
            _onUbxTMode3Packet = new PropertyObservable<UbxTimeModeConfiguration>(GetUbxCfgTMode3, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
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
            Disposable.Add(_onUbxVersionPacket);
            Disposable.Add(_onUbxTMode3Packet);
        }

        public IObservable<RtcmV3MessageBase> OnRtcm => _onRtcmPacket;
        public IObservable<RawRtcmV3Message> OnRawRtcm => _onRawRtcmPacket;
        public IObservable<UbxMessageBase> OnUbx => _onUbxPacket;
        public IObservable<UbxAck> OnAck => _onUbxAckPacket;
        public IObservable<UbxNak> OnNak => _onUbxNakPacket;
        
        public IRxValue<GeoPoint> OnLocation => _onMovingBasePosition;
        public IRxValue<UbxNavSurveyIn> OnSurveyIn => _onSurveyIn;
        public IRxValue<UbxNavPvt> OnMovingBase => _onUbxNavPvt;
        public IRxValue<UbxVelocitySolutionInNED> OnVelocitySolution => _onVelocitySolution;
        public IRxValue<UbxMonitorHardware> OnHwInfo => _onUbxMonHwPacket;
        
        public IObservable<UbxTimeModeConfiguration> OnUbxTimeMode => _onUbxTMode3Packet;
        public IObservable<UbxMonitorVersion> OnVersion => _onUbxVersionPacket;
        public IObservable<UbxInfWarning> UbxWarning => _onUbxInfWarningPacket;
        public IRxValue<bool> OnReboot => _onReboot;

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
            _onUbxPacket.Where(_ => _.MessageId == 0x0112)
                .Select(_ => _ as UbxVelocitySolutionInNED).Subscribe(_onVelocitySolution, DisposeCancel);

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

        public Task<bool> ExecuteCommand<TPacket>(CancellationToken cancel)
            where TPacket : UbxMessageBase, new()
        {
            var packet = new TPacket();
            return ExecuteCommand(packet, cancel);
        }

        public async Task<bool> ExecuteCommand<TPacket>(TPacket packet, CancellationToken cancel)
            where TPacket : UbxMessageBase
        {
            byte currentAttempt = 0;
            while (currentAttempt < AttemptCount)
            {
                ++currentAttempt;
                try
                {
                    using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
                    linkedCancel.CancelAfter(CommandTimeoutMs);
                    var tcs = new TaskCompletionSource<bool>();
                    using var c1 = linkedCancel.Token.Register(() => tcs.TrySetCanceled());
                    
                    using var subscribeAck = _onUbxAckPacket
                        .FirstAsync(_ => packet.Class == _.AckClassId && packet.SubClass == _.AckSubclassId)
                        .Subscribe(_ => tcs.TrySetResult(true));
                    using var subscribeNak = _onUbxNakPacket
                        .FirstAsync(_ => packet.Class == _.AckClassId && packet.SubClass == _.AckSubclassId)
                        .Subscribe(_ => tcs.TrySetResult(false));

                    await _connection.Send(packet, linkedCancel.Token).ConfigureAwait(false);
                    var result = await tcs.Task.ConfigureAwait(false);
                    return result;
                }
                catch (TaskCanceledException)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        throw;
                    }
                }
            }

            throw new TimeoutException(string.Format(
                "Timeout to execute command '{0}' with '{1}' attempts (timeout {1} times by {2:g} )", nameof(TPacket),
                currentAttempt, CommandTimeoutMs));

        }

        public async Task<TPacket> GetUbxPacket<TPacket>(CancellationToken cancel)
            where TPacket : UbxMessageBase, new()
        {
            var p = new TPacket();
            var packet = p.GetRequest();

            byte currentAttempt = 0;
            while (currentAttempt < AttemptCount)
            {
                ++currentAttempt;
                try
                {
                    using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
                    linkedCancel.CancelAfter(CommandTimeoutMs);
                    var tcs = new TaskCompletionSource<TPacket>();
                    using var c1 = linkedCancel.Token.Register(() => tcs.TrySetCanceled());

                    using var subscribe = _onUbxPacket
                        .FirstAsync(_ => _.MessageId == p.MessageId)
                        .Subscribe(_ => tcs.TrySetResult(_ as TPacket));


                    await _connection.Send(packet, linkedCancel.Token).ConfigureAwait(false);
                    var result = await tcs.Task.ConfigureAwait(false);
                    return result;
                }
                catch (TaskCanceledException)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        throw;
                    }
                }
            }

            throw new TimeoutException(string.Format(
                "Request response timeout '{0}' with '{1}' attempts (timeout {1} times by {2:g} )", nameof(TPacket),
                currentAttempt, CommandTimeoutMs));
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

        public Task<UbxTimeModeConfiguration> GetUbxCfgTMode3(CancellationToken cancel = default)
        {
            return GetUbxPacket<UbxTimeModeConfiguration>(cancel);
        }

        public Task<UbxMonitorVersion> GetUbxMonVersion(CancellationToken cancel = default)
        {
            return GetUbxPacket<UbxMonitorVersion>(cancel);
        }

        #endregion

        #region TimeMode

        public async Task StopSurveyInTimeMode(CancellationToken cancel)
        {
            _logger.Info($"Disable RTK base survey in time mode");
            if (!await ExecuteCommand<UbxMovingBaseStation>(cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", nameof(StopSurveyInTimeMode)));
        }

        public async Task SetSurveyInTimeMode(uint duration, double accLimit, CancellationToken cancel)
        {
            // Check current TimeMode
            var timeMode = await GetUbxCfgTMode3(cancel).ConfigureAwait(false);
            if (timeMode.Mode == UbxTimeModeConfiguration.ReceiverMode.Disabled)
            {
                await SetStationaryMode(true, cancel);
                await SetupRtcmMSM4Rate(_msmMsgRate, cancel);
                await SetupRtcmMSM7Rate(0, cancel);
            }
            
            _logger.Info($"Setup RTK base survey in mode duration:{duration}, accLimit:{accLimit}");
            if (!await ExecuteCommand(new UbxSurveyInBaseStation(duration, accLimit), cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", nameof(SetSurveyInTimeMode)));

            if (timeMode.Mode == UbxTimeModeConfiguration.ReceiverMode.FixedMode)
                await RebootReceiver(cancel);
        }

        public async Task SetFixedBaseTimeMode(GeoPoint location, double accLimit, CancellationToken cancel)
        {
            // Check current TimeMode
            var timeMode = await GetUbxCfgTMode3(cancel).ConfigureAwait(false);
            if (timeMode.Mode == UbxTimeModeConfiguration.ReceiverMode.Disabled)
            {
                await SetStationaryMode(true, cancel);
                await SetupRtcmMSM4Rate(_msmMsgRate, cancel);
                await SetupRtcmMSM7Rate(0, cancel);
            }
            _logger.Info($"Setup RTK base TMode3 fixed position mode location:{location}, accLimit:{accLimit}");
            if (!await ExecuteCommand(new UbxFixedBaseStation(location, accLimit), cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", nameof(UbxFixedBaseStation)));
        }

        public async Task SetStandaloneTimeMode(CancellationToken cancel)
        {
            // Check current TimeMode
            var timeMode = await GetUbxCfgTMode3(cancel).ConfigureAwait(false);
            if (timeMode.Mode != UbxTimeModeConfiguration.ReceiverMode.Disabled)
            {
                await SetupRtcmMSM4Rate(0, cancel);
                await SetupRtcmMSM7Rate(_msmMsgRate, cancel);
            }
            _logger.Info($"Setup RTK base TMode3 moving base mode");
            if (!await ExecuteCommand<UbxMovingBaseStation>(cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", nameof(SetStandaloneTimeMode)));
            if (timeMode.Mode != UbxTimeModeConfiguration.ReceiverMode.Disabled)
                await SetStationaryMode(false, cancel);
            await RebootReceiver(cancel);
        }

        #endregion

        #region Config

        public async Task SetRtcmRate(byte msgRate, CancellationToken cancel)
        {
            _msmMsgRate = msgRate;
            var timeMode = await GetUbxCfgTMode3(cancel);
            if (timeMode.Mode == UbxTimeModeConfiguration.ReceiverMode.Disabled)
            {
                await SetupRtcmMSM4Rate(0, cancel);
                await SetupRtcmMSM7Rate(msgRate, cancel);
            }
            else
            {
                await SetupRtcmMSM4Rate(msgRate, cancel);
                await SetupRtcmMSM7Rate(0, cancel);
            }
        }

        #region RTCM MSM

        private async Task SetupRtcmMSM4Rate(byte msgRate, CancellationToken cancel)
        {
            // 1074
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x4A, msgRate, cancel);

            // 1084
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x54, msgRate, cancel);

            // 1094
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x5E, msgRate, cancel);

            // 1124
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x7C, msgRate, cancel);
        }

        private async Task SetupRtcmMSM7Rate(byte msgRate, CancellationToken cancel)
        {
            // 1077
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x4D, msgRate, cancel);

            // 1087
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x57, msgRate, cancel);

            // 1097
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x61, msgRate, cancel);

            // 1127
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x7F, msgRate, cancel);
        }

        #endregion

        public async Task SetupByDefault(CancellationToken cancel)
        {
            await SetPorts(cancel);

            await SetRateTo1Hz(cancel);
            await SetStationaryMode(false, cancel);
            await TurnOffNMEA(cancel);

            // mon-ver
            // await PollMsg((byte)UbxHelper.ClassIDs.MON, 0x04, cancel);

            // surveyin msg - for feedback
            await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x3B, 1, cancel);
            
            // pvt msg - for feedback
            await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x07, 1, cancel);

            // 1005 - 5s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0x05, 5, cancel);

            await SetupRtcmMSM4Rate(_msmMsgRate, cancel);
            await SetupRtcmMSM7Rate(0, cancel);

            // 1230 - 5s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0xE6, 5, cancel);

            // NAV-VELNED - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.NAV, 0x12, 1, cancel);

            // rxm-raw/rawx - 1s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x15, 1, cancel);
            //await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x10, 1, cancel);

            // rxm-sfrb/sfrb - 2s
            await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x13, 2, cancel);
            //await SetMessageRate((byte)UbxHelper.ClassIDs.RXM, 0x11, 2, cancel);

            // mon-hw - 2s
            await SetMessageRate((byte)UbxHelper.ClassIDs.MON, 0x09, 2, cancel);

            await Task.Delay(100, cancel);
        }

        private async Task SetPorts(CancellationToken cancel)
        {
            if (!await ExecuteCommand(
                        new UbxPortConfiguration(PortType.Uart) { SerialPortConfig = { BoundRate = 115200 } }, cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", "SetUartPortConfiguration"));
            if (!await ExecuteCommand(new UbxPortConfiguration(PortType.Usb), cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", "SetUsbPortConfiguration"));

        }

        private async Task SetRateTo1Hz(CancellationToken cancel)
        {
            if (!await ExecuteCommand(new UbxRateSettings {RateHz = 1.0}, cancel).ConfigureAwait(false))
                throw new Exception(string.Format("Execute command '{0}' error", nameof(UbxRateSettings)));
        }

        public async Task SetStationaryMode(bool movingBase, CancellationToken cancel)
        {
            if (!movingBase)
            {
                if (!await ExecuteCommand(new UbxNavigationEngineSettings(UbxNavigationEngineSettings.ModelEnum.Stationary), cancel).ConfigureAwait(false))
                    throw new Exception(string.Format("Execute command '{0}' error", nameof(SetStationaryMode)));

                // 4072
                await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0xFE, 0, cancel);
            }
            else
            {
                if (!await ExecuteCommand(new UbxNavigationEngineSettings(UbxNavigationEngineSettings.ModelEnum.Portable), cancel).ConfigureAwait(false))
                    throw new Exception(string.Format("Execute command '{0}' error", nameof(SetStationaryMode)));
                // 4072
                await SetMessageRate((byte)UbxHelper.ClassIDs.RTCM3, 0xFE, _msmMsgRate, cancel);
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
            if (!await ExecuteCommand(new UbxMessageConfiguration(msgClass, msgSubClass, msgRate), cancel).ConfigureAwait(false))
                throw new Exception(string.Format(
                    "Execute command '{0}' with param MsgClass 0x'{1:X2}' MsgSubClass 0x'{2:X2}' Rate '{3}' error",
                    nameof(UbxMessageConfiguration), msgClass, msgSubClass, msgRate));

        }

        private async Task RebootReceiver(CancellationToken cancel)
        {
            if (Interlocked.CompareExchange(ref _rebootNotComplete, 1, 0) != 0) return;
            
            try
            {
                _onReboot.OnNext(true);
                _logger.Info($"Save receiver configuration to RAM");
                if (!await ExecuteCommand<UbxSaveConfigurations>(cancel).ConfigureAwait(false))
                    throw new Exception(string.Format("Execute command '{0}' error", nameof(UbxSaveConfigurations)));

                _logger.Info($"Reboot receiver (GNSS Only)");
                await _connection.Send(new UbxColdSoftwareGnssReset(), cancel).ConfigureAwait(false);
                //await _connection.Send(new UbxHotHardwareReset(), cancel).ConfigureAwait(false);

                await WaitRebootReceiver(cancel);
                _logger.Info($"Start receiver");
                _logger.Info($"Load receiver configuration");
                if (!await ExecuteCommand<UbxLoadConfigurations>(cancel).ConfigureAwait(false))
                    throw new Exception(string.Format("Execute command '{0}' error", nameof(UbxLoadConfigurations)));
                _onReboot.OnNext(false);

                return;
            }
            catch (TaskCanceledException)
            {
                if (cancel.IsCancellationRequested)
                {
                    throw;
                }
            }
            finally
            {
                Interlocked.Exchange(ref _rebootNotComplete, 0);
            }

            throw new TimeoutException("Reboot receiver timeout");
        }

        private async Task WaitRebootReceiver(CancellationToken cancel)
        {
            using var linkedCancel = CancellationTokenSource.CreateLinkedTokenSource(cancel);
            linkedCancel.CancelAfter(TimeSpan.FromSeconds(5));
            var tcs = new TaskCompletionSource<GnssMessageBase>();
            using var c1 = linkedCancel.Token.Register(() => tcs.TrySetCanceled());

            using var subscribe = _connection.OnMessage
                .FirstAsync(_ => true)
                .Subscribe(_ => tcs.TrySetResult(_));
            await tcs.Task.ConfigureAwait(false);
        }

        #endregion


    }
}