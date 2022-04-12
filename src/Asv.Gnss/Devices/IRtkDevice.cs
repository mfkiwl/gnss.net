using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;
using NLog;

namespace Asv.Gnss
{
    public interface IRtkDevice
    {
        IObservable<RawRtcmV3Message> OnRawRtcm { get; }

        // IRxValue<BaseMode> OnMode { get; }
        // IRxValue<GeoPoint> OnLocation { get; }
        // IRxValue<RtkStationState> 


        #region TimeMode

        Task StopSurveyInTimeMode(CancellationToken cancel);
        Task SetSurveyInTimeMode(uint duration, double accLimit, CancellationToken cancel);
        Task SetFixedBaseTimeMode(GeoPoint location, double accLimit, CancellationToken cancel);
        Task SetStandaloneTimeMode(CancellationToken cancel);

        Task<BaseStationInfo> GetInfo(CancellationToken cancel = default);
        Task<ObservationInfo> GetObservationInfo(CancellationToken cancel = default);

        #endregion

        Task SetRtcmRate(byte msgRate, CancellationToken cancel);
    }

    public enum TimeMode
    {
        Standalone = 0,
        SurveyIn = 1,
        FixedMode = 2
    }

    public interface IRtkService
    {
        Task Reboot(CancellationToken cancel);
        Task StartFixedBase(StartFixedBaseRequest request, CancellationToken cancel = default);
        Task StartSurvey(StartSurveyInRequest request, CancellationToken cancel = default);
        Task StopSurvey(CancellationToken cancel = default);
        Task<BaseStationInfo> GetInfo(CancellationToken cancel = default);
        Task<ObservationInfo> GetObservationInfo(CancellationToken cancel = default);
    }

    public class RtkModuleConfig
    {
        public bool IsEnabled { get; set; } = true;
    }

    public class RtkService : DisposableOnceWithCancel, IRtkService
    {
        private readonly IRtkDevice _device;
        private readonly IConfiguration _cfgSrv;
        private readonly RtkModuleConfig _cfg;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private volatile int _bytesRtkBytesPerSecond;
        

        public RtkService(IRtkDevice device, IConfiguration cfgSrv)
        {
            _device = device;
            _cfgSrv = cfgSrv;
            _cfg = _cfgSrv.Get<RtkModuleConfig>();
            _device.OnRawRtcm.Where(_ => _cfg.IsEnabled).Subscribe(SendData, DisposeCancel);
            _device.OnRawRtcm.Where(_ => _cfg.IsEnabled).Select(_ => _.RawData.Length).Buffer(TimeSpan.FromSeconds(1))
                .Subscribe(_ => Interlocked.Exchange(ref _bytesRtkBytesPerSecond, _.Sum()));
        }

        private void SendData(RawRtcmV3Message bytes)
        {

            Console.WriteLine($"RTCM{bytes.MessageId}");
        }

        public Task Reboot(CancellationToken cancel)
        {
            _logger.Info($"Reboot requested");
            Task.Factory.StartNew(async () =>
                {
                    await Task.Delay(1000, cancel);
                    Environment.Exit(0);
                }
                , TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }

        public Task StartFixedBase(StartFixedBaseRequest request, CancellationToken cancel = default)
        {
            _logger.Info($"Start fixed base Lat:{request.Latitude}, Lon:{request.Longitude}, Alt:{request.Altitude} position with acc:{request.AccLimit:F2} m");
            return _device.SetFixedBaseTimeMode(new GeoPoint(request.Latitude, request.Longitude, request.Altitude), request.AccLimit, cancel);
        }

        public Task StartSurvey(StartSurveyInRequest request, CancellationToken cancel = default)
        {
            _logger.Info($"Start survey in duration:{request.Duration} sec, acc:{request.AccLimit:F2} m");
            return _device.SetSurveyInTimeMode(request.Duration, request.AccLimit, cancel);
        }

        public Task StopSurvey(CancellationToken cancel = default)
        {
            _logger.Info($"Stop survey in");
            return _device.StopSurveyInTimeMode(cancel);
        }

        public Task<BaseStationInfo> GetInfo(CancellationToken cancel = default)
        {
            throw new NotImplementedException();
        }

        public Task<ObservationInfo> GetObservationInfo(CancellationToken cancel = default)
        {
            throw new NotImplementedException();
        }
    }

    public class StartFixedBaseRequest : SpanBitCompactSerializableBase
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double AccLimit { get; set; }

        public override int GetBitSize()
        {
            return 112;
        }

        public override void Deserialize(ref ReadOnlySpan<byte> buffer, ref uint bitPosition)
        {
            Latitude = GnssRecordDataModelHelper.GetLatitude32Bit(ref buffer, ref bitPosition);
            Longitude = GnssRecordDataModelHelper.GetLongitude32Bit(ref buffer, ref bitPosition);
            Altitude = GnssRecordDataModelHelper.GetAltitude24Bit(ref buffer, ref bitPosition);
            AccLimit = GnssRecordDataModelHelper.GetAccuracy24Bit(ref buffer, ref bitPosition);
        }

        public override void Serialize(ref Span<byte> buffer, ref uint bitPosition)
        {
            GnssRecordDataModelHelper.SetLatitude32Bit(ref buffer, ref bitPosition, Latitude);
            GnssRecordDataModelHelper.SetLongitude32Bit(ref buffer, ref bitPosition, Longitude);
            GnssRecordDataModelHelper.SetAltitude24Bit(ref buffer, ref bitPosition, Altitude);
            GnssRecordDataModelHelper.SetAccuracy24Bit(ref buffer, ref bitPosition, AccLimit);
        }
    }

    public class StartSurveyInRequest : SpanBitCompactSerializableBase
    {
        public uint Duration { get; set; }
        public double AccLimit { get; set; }
        public override int GetBitSize()
        {
            return 48;
        }

        public override void Deserialize(ref ReadOnlySpan<byte> buffer, ref uint bitPosition)
        {
            AccLimit = GnssRecordDataModelHelper.GetAccuracy24Bit(ref buffer, ref bitPosition);
            Duration = GnssRecordDataModelHelper.GetDuration24Bit(ref buffer, ref bitPosition) ?? 60;
        }

        public override void Serialize(ref Span<byte> buffer, ref uint bitPosition)
        {
            GnssRecordDataModelHelper.SetAccuracy24Bit(ref buffer, ref bitPosition, AccLimit);
            GnssRecordDataModelHelper.SetDuration24Bit(ref buffer, ref bitPosition, Duration);
        }
    }

    public class ObservationInfo : SpanBitCompactSerializableBase
    {
        public byte? Gps { get; set; }
        public byte? Sbas { get; set; }
        public byte? Glonass { get; set; }
        public byte? BeiDou { get; set; }
        public byte? Galileo { get; set; }
        public byte? Imes { get; set; }
        public byte? Qzss { get; set; }
        public ushort Noise { get; set; }
        public double AgcMonitor { get; set; } = double.NaN;
        public double JammingIndicator { get; set; } = double.NaN;

        public override int GetBitSize()
        {
            return 104;
        }

        public override void Deserialize(ref ReadOnlySpan<byte> buffer, ref uint bitPosition)
        {
            Gps = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            Sbas = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            Glonass = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            BeiDou = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            Galileo = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            Imes = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            Qzss = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition); bitPosition += 7;
            Noise = (ushort)SpanBitHelper.GetBitU(ref buffer, ref bitPosition, 16);
            AgcMonitor = GnssRecordDataModelHelper.GetAgcJam14Bit(ref buffer, ref bitPosition);
            JammingIndicator = GnssRecordDataModelHelper.GetAgcJam14Bit(ref buffer, ref bitPosition); bitPosition += 4;
        }

        public override void Serialize(ref Span<byte> buffer, ref uint bitPosition)
        {
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, Gps);
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, Sbas);
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, Glonass);
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, BeiDou);
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, Galileo);
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, Imes);
            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, Qzss); bitPosition += 7;
            SpanBitHelper.SetBitU(ref buffer, ref bitPosition, 16, Noise);
            GnssRecordDataModelHelper.SetAgcJam14Bit(ref buffer, ref bitPosition, AgcMonitor);
            GnssRecordDataModelHelper.SetAgcJam14Bit(ref buffer, ref bitPosition, JammingIndicator); bitPosition += 4;
        }
    }

    public enum TimeModeEnum
    {
        Standalone,
        SurveyIn,
        FixedBase
    }

    public static class GnssRecordDataModelHelper
    {
        #region GNSS Latitude\Longitude

        /// <summary>
        /// 0.0005 arcsecond
        /// </summary>
        public const double ArcSecond = 1.388888888888889e-7;
        public const double MaxLat = 90;
        public const double MinLat = -90.0;
        public const double MaxLon = 180;
        public const double MinLon = -180.0;
        public const uint LatLonBitSize = 32;

        public static double GetLatitude32Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return SpanBitHelper.GetFixedPointS32Bit(ref buffer, ref bitIndex, ArcSecond, 0, MaxLat, MinLat);
        }

        public static void SetLatitude32Bit(ref Span<byte> buffer, ref uint bitIndex, double latitude)
        {
            SpanBitHelper.SetFixedPointS32Bit(ref buffer, ref bitIndex, latitude, ArcSecond, 0, MaxLat, MinLat);
        }

        public static double GetLongitude32Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return SpanBitHelper.GetFixedPointS32Bit(ref buffer, ref bitIndex, ArcSecond, 0, MaxLon, MinLon);
        }

        public static void SetLongitude32Bit(ref Span<byte> buffer, ref uint bitIndex, double longitude)
        {
            SpanBitHelper.SetFixedPointS32Bit(ref buffer, ref bitIndex, longitude, ArcSecond, 0, MaxLon, MinLon);
        }

        #endregion

        #region GNSS Altitude

        private const double AltFraction = 0.01f;
        public const double MaxAlt = SpanBitHelper.FixedPointS24Max * AltFraction;
        public const double MinAlt = SpanBitHelper.FixedPointS24Min * AltFraction;
        public const uint GnssAltitudeBitSize = 24;

        public static double GetAltitude24Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return SpanBitHelper.GetFixedPointS24Bit(ref buffer, ref bitIndex, AltFraction);
        }

        public static void SetAltitude24Bit(ref Span<byte> buffer, ref uint bitIndex, double altitude)
        {
            SpanBitHelper.SetFixedPointS24Bit(ref buffer, ref bitIndex, altitude, AltFraction);
        }

        #endregion

        #region Agc/Jamming

        private const double AgcJamFraction = 0.01;
        public const double MaxAgcJam = 10000.0 * AgcJamFraction;
        public static double GetAgcJam14Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return SpanBitHelper.GetFixedPointS14Bit(ref buffer, ref bitIndex, AgcJamFraction);
        }

        public static void SetAgcJam14Bit(ref Span<byte> buffer, ref uint bitIndex, double agcJam)
        {
            SpanBitHelper.SetFixedPointS14Bit(ref buffer, ref bitIndex, agcJam, AgcJamFraction, 0, MaxAgcJam, 0.0);
        }

        #endregion

        #region Accuracy

        private const double AccFraction = 0.0001;
        public const double MaxAcc = 2 * SpanBitHelper.FixedPointS24Max * AccFraction;
        public const double MinAcc = 0.0;
        public const uint AccuracyBitSize = 24;

        public static double GetAccuracy24Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return SpanBitHelper.GetFixedPointS24Bit(ref buffer, ref bitIndex, AccFraction);
        }

        public static void SetAccuracy24Bit(ref Span<byte> buffer, ref uint bitIndex, double accuracy)
        {
            SpanBitHelper.SetFixedPointS24Bit(ref buffer, ref bitIndex, accuracy, AccFraction, 0.0, MaxAcc, MinAcc);
        }

        #endregion

        #region Duration

        public const int DurNull = 0xFFFFFF;
        public const int MaxDur = 0xFFFFFE;
        public const int MinDur = 0;
        public const uint DurationBitSize = 24;

        public static uint? GetDuration24Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            var result = SpanBitHelper.GetBitU(ref buffer, ref bitIndex, DurationBitSize);
            return result == DurNull ? null : result;
        }

        public static void SetDuration24Bit(ref Span<byte> buffer, ref uint bitIndex, uint? duration)
        {
            duration ??= DurNull;
            SpanBitHelper.SetBitU(ref buffer, ref bitIndex, DurationBitSize, duration.Value);
        }

        #endregion

        #region GNSS HDOP\VDOP\PDOP

        public const double DopFraction = 0.1;
        public const double MaxDop = 2.0 * SpanBitHelper.FixedPointS7Max * DopFraction;
        public const double MinDop = 0;
        public const uint DopBitSize = 7;

        public static double GetGnssDop7Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return SpanBitHelper.GetFixedPointS7Bit(ref buffer, ref bitIndex, DopFraction, SpanBitHelper.FixedPointS7Max * DopFraction);
        }

        public static void SetGnssDop7Bit(ref Span<byte> buffer, ref uint bitIndex, double dop)
        {
            if (dop < 0) throw new ArgumentOutOfRangeException(nameof(dop));
            SpanBitHelper.SetFixedPointS7Bit(ref buffer, ref bitIndex, dop, DopFraction, SpanBitHelper.FixedPointS7Max * DopFraction);
        }
        #endregion

        #region Observation Count

        public const int ObservationCountNull = 0x7F;
        public const byte MaxObservationCount = 0x7E;
        public const uint ObservationCountBitSize = 7;

        public static byte? GetObservationCount7Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            var result = (byte)SpanBitHelper.GetBitU(ref buffer, ref bitIndex, ObservationCountBitSize);
            return result == ObservationCountNull ? null : result;
        }

        public static void SetObservationCount7Bit(ref Span<byte> buffer, ref uint bitIndex, byte? count)
        {
            count = count switch
            {
                null => ObservationCountNull,
                > MaxObservationCount => MaxObservationCount,
                _ => count
            };
            SpanBitHelper.SetBitU(ref buffer, ref bitIndex, ObservationCountBitSize, count.Value);
        }

        #endregion

        #region Time mode

        public const uint TimeModeBitSize = 2;

        public static TimeModeEnum GetTimeMode2Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            return (TimeModeEnum)SpanBitHelper.GetBitU(ref buffer, ref bitIndex, TimeModeBitSize);
        }

        public static void SetTimeMode2Bit(ref Span<byte> buffer, ref uint bitIndex, TimeModeEnum mode)
        {
            SpanBitHelper.SetBitU(ref buffer, ref bitIndex, TimeModeBitSize, (uint)mode);
        }

        #endregion

        #region Bool?

        public const uint BooleanNull = 0x3;
        public const uint BooleanBitSize = 2;

        public static bool? GetBoolean2Bit(ref ReadOnlySpan<byte> buffer, ref uint bitIndex)
        {
            var result = SpanBitHelper.GetBitU(ref buffer, ref bitIndex, BooleanBitSize);
            if (result == BooleanNull) return null;
            return result != 0;
        }

        public static void SetBoolean2Bit(ref Span<byte> buffer, ref uint bitIndex, bool? value)
        {
            var v = value == null ? BooleanNull : value.Value ? 1u : 0;
            SpanBitHelper.SetBitU(ref buffer, ref bitIndex, BooleanBitSize, v);
        }

        #endregion
    }

    public class BaseStationInfo : SpanBitCompactSerializableBase
    {
        public bool IsEnabled { get; set; }
        public bool Vehicle { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public uint? Observations { get; set; } = null;
        public double PDOP { get; set; }


        public TimeModeEnum TimeMode { get; set; }
        public bool? Active { get; set; } = null;
        public bool? Valid { get; set; } = null;

        public double Accuracy { get; set; } = double.NaN;
        public uint? Duration { get; set; } = null;
        
        public uint RtkSpeedBps { get; set; }

        public override int GetBitSize()
        {
            return 184;
        }

        public override void Deserialize(ref ReadOnlySpan<byte> buffer, ref uint bitPosition)
        {
            IsEnabled = SpanBitHelper.GetBitU(ref buffer, ref bitPosition, 1) == 1;
            Vehicle = SpanBitHelper.GetBitU(ref buffer, ref bitPosition, 1) == 1; bitPosition += 6;
            
            Latitude = GnssRecordDataModelHelper.GetLatitude32Bit(ref buffer, ref bitPosition);
            Longitude = GnssRecordDataModelHelper.GetLongitude32Bit(ref buffer, ref bitPosition);
            Altitude = GnssRecordDataModelHelper.GetAltitude24Bit(ref buffer, ref bitPosition);
            
            Observations = GnssRecordDataModelHelper.GetObservationCount7Bit(ref buffer, ref bitPosition);
            PDOP = GnssRecordDataModelHelper.GetGnssDop7Bit(ref buffer, ref bitPosition);

            TimeMode = GnssRecordDataModelHelper.GetTimeMode2Bit(ref buffer, ref bitPosition);
            Active = GnssRecordDataModelHelper.GetBoolean2Bit(ref buffer, ref bitPosition);
            Valid = GnssRecordDataModelHelper.GetBoolean2Bit(ref buffer, ref bitPosition); bitPosition += 4;

            Accuracy = GnssRecordDataModelHelper.GetAccuracy24Bit(ref buffer, ref bitPosition);
            Duration = GnssRecordDataModelHelper.GetDuration24Bit(ref buffer, ref bitPosition);

            RtkSpeedBps = SpanBitHelper.GetBitU(ref buffer, ref bitPosition, 16);
        }

        public override void Serialize(ref Span<byte> buffer, ref uint bitPosition)
        {
            SpanBitHelper.SetBitU(ref buffer, ref bitPosition, 1, IsEnabled ? 1 : 0);
            SpanBitHelper.SetBitU(ref buffer, ref bitPosition, 1, Vehicle ? 1 : 0); bitPosition += 6;

            GnssRecordDataModelHelper.SetLatitude32Bit(ref buffer, ref bitPosition, Latitude);
            GnssRecordDataModelHelper.SetLongitude32Bit(ref buffer, ref bitPosition, Longitude);
            GnssRecordDataModelHelper.SetAltitude24Bit(ref buffer, ref bitPosition, Altitude);

            GnssRecordDataModelHelper.SetObservationCount7Bit(ref buffer, ref bitPosition, (byte?)Observations);
            GnssRecordDataModelHelper.SetGnssDop7Bit(ref buffer, ref bitPosition, PDOP);

            GnssRecordDataModelHelper.SetTimeMode2Bit(ref buffer, ref bitPosition, TimeMode);
            GnssRecordDataModelHelper.SetBoolean2Bit(ref buffer, ref bitPosition, Active);
            GnssRecordDataModelHelper.SetBoolean2Bit(ref buffer, ref bitPosition, Valid); bitPosition += 4;

            GnssRecordDataModelHelper.SetAccuracy24Bit(ref buffer, ref bitPosition, Accuracy);
            GnssRecordDataModelHelper.SetDuration24Bit(ref buffer, ref bitPosition, Duration);

            SpanBitHelper.SetBitU(ref buffer, ref bitPosition, 16, RtkSpeedBps);
        }
    }
}