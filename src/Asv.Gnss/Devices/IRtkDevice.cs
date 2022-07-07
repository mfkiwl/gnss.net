using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;

namespace Asv.Gnss
{
    public interface IRtkDevice
    {
        Task Init(CancellationToken cancel = default);

        IObservable<RawRtcmV3Message> OnRawRtcm { get; }

        #region TimeMode

        Task StopSurveyInTimeMode(CancellationToken cancel);
        Task StartSurveyInTimeMode(uint duration, double accLimit, CancellationToken cancel);
        Task SetFixedBaseTimeMode(GeoPoint location, double accLimit, CancellationToken cancel);
        Task SetStandaloneTimeMode(CancellationToken cancel);

        Task<RtkInfo> GetRtkInfo(CancellationToken cancel = default);

        #endregion

        Task SetRtcmRate(byte msgRate, CancellationToken cancel);
    }

    public class FixedBaseArgs : SpanBitCompactSerializableBase
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public double AccLimit { get; set; }

        public override int GetBitSize()
        {
            return 120;
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            Latitude = RtkRecordDataModelHelper.GetLatitude32Bit(buffer, ref bitPosition);
            Longitude = RtkRecordDataModelHelper.GetLongitude32Bit(buffer, ref bitPosition);
            Altitude = RtkRecordDataModelHelper.GetAltitude24Bit(buffer, ref bitPosition);
            AccLimit = RtkRecordDataModelHelper.GetAccuracy32Bit(buffer, ref bitPosition);
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            RtkRecordDataModelHelper.SetLatitude32Bit(buffer, ref bitPosition, Latitude);
            RtkRecordDataModelHelper.SetLongitude32Bit(buffer, ref bitPosition, Longitude);
            RtkRecordDataModelHelper.SetAltitude24Bit(buffer, ref bitPosition, Altitude);
            RtkRecordDataModelHelper.SetAccuracy32Bit(buffer, ref bitPosition, AccLimit);
        }
    }

    public class FixedBaseResult : SpanBitCompactSerializableBase
    {
        public override int GetBitSize()
        {
            return 8;
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            IsSuccess = (SpanBitHelper.GetBitU(buffer, ref bitPosition, 8) & 0x01) != 0;
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            SpanBitHelper.SetBitU(buffer, ref bitPosition, 8, IsSuccess ? 1 : 0);
        }

        public bool IsSuccess { get; set; }
    }

    public class SurveyInArgs : SpanBitCompactSerializableBase
    {
        public bool IsActivate { get; set; }
        public uint Duration { get; set; }
        public double AccLimit { get; set; }
        
        public override int GetBitSize()
        {
            return 64;
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            IsActivate = SpanBitHelper.GetBitU(buffer, ref bitPosition, 8) != 0;
            AccLimit = RtkRecordDataModelHelper.GetAccuracy32Bit(buffer, ref bitPosition);
            Duration = RtkRecordDataModelHelper.GetDuration24Bit(buffer, ref bitPosition) ?? 60;
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            SpanBitHelper.SetBitU(buffer, ref bitPosition, 8, IsActivate ? 1 : 0);
            RtkRecordDataModelHelper.SetAccuracy32Bit(buffer, ref bitPosition, AccLimit);
            RtkRecordDataModelHelper.SetDuration24Bit(buffer, ref bitPosition, Duration);
        }
    }

    public class SurveyInResult : SpanBitCompactSerializableBase
    {
        public override int GetBitSize()
        {
            return 8;
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            IsSuccess = (SpanBitHelper.GetBitU(buffer, ref bitPosition, 8) & 0x01) != 0;
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            SpanBitHelper.SetBitU(buffer, ref bitPosition, 8, IsSuccess ? 1 : 0);
        }

        public bool IsSuccess { get; set; }
    }

    public class RtkInfo : SpanBitCompactSerializableBase
    {
        public BaseStationInfo BaseStation { get; }
        public ObservationInfo Observation { get; }

        public RtkInfo()
        {
            BaseStation = new BaseStationInfo();
            Observation = new ObservationInfo();
        }

        public RtkInfo(BaseStationInfo baseStation, ObservationInfo observation)
        {
            BaseStation = baseStation;
            Observation = observation;
        }

        public override int GetBitSize()
        {
            return BaseStation.GetBitSize() + Observation.GetBitSize();
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            BaseStation.Deserialize(buffer, ref bitPosition);
            Observation.Deserialize(buffer, ref bitPosition);
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            BaseStation.Serialize(buffer, ref bitPosition);
            Observation.Serialize(buffer, ref bitPosition);
        }
    }

    public class BaseStationInfo : SpanBitCompactSerializableBase
    {
        public bool IsEnabled { get; set; }
        public byte Vehicle { get; set; }
        public double Latitude { get; set; } = double.NaN;
        public double Longitude { get; set; } = double.NaN;
        public double Altitude { get; set; } = double.NaN;
        public uint? Observations { get; set; } = null;
        public double PDOP { get; set; }

        public TimeModeEnum TimeMode { get; set; } = TimeModeEnum.Unknown;
        public GnssFixType FixType { get; set; } = GnssFixType.Unknown;

        #region SurvyIn
        public bool? Active { get; set; } = null;
        public bool? Valid { get; set; } = null;
        public double Accuracy { get; set; } = double.NaN;
        public uint? Duration { get; set; } = null;
        #endregion

        public uint RtkSpeedBps { get; set; } = 0;

        public override int GetBitSize()
        {
            return 200;
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            IsEnabled = SpanBitHelper.GetBitU(buffer, ref bitPosition, 1) == 1;
            Vehicle = (byte)SpanBitHelper.GetBitU(buffer, ref bitPosition, 7);
            
            Latitude = RtkRecordDataModelHelper.GetLatitude32Bit(buffer, ref bitPosition);
            Longitude = RtkRecordDataModelHelper.GetLongitude32Bit(buffer, ref bitPosition);
            Altitude = RtkRecordDataModelHelper.GetAltitude24Bit(buffer, ref bitPosition);
            
            Observations = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            PDOP = RtkRecordDataModelHelper.GetGnssDop10Bit(buffer, ref bitPosition);

            TimeMode = RtkRecordDataModelHelper.GetTimeMode2Bit(buffer, ref bitPosition);
            FixType = RtkRecordDataModelHelper.GetFixType3Bit(buffer, ref bitPosition);

            Active = RtkRecordDataModelHelper.GetBoolean2Bit(buffer, ref bitPosition);
            Valid = RtkRecordDataModelHelper.GetBoolean2Bit(buffer, ref bitPosition); bitPosition += 6;

            Accuracy = RtkRecordDataModelHelper.GetAccuracy32Bit(buffer, ref bitPosition);
            Duration = RtkRecordDataModelHelper.GetDuration24Bit(buffer, ref bitPosition);

            RtkSpeedBps = SpanBitHelper.GetBitU(buffer, ref bitPosition, 16);
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            SpanBitHelper.SetBitU(buffer, ref bitPosition, 1, IsEnabled ? 1 : 0);
            SpanBitHelper.SetBitU(buffer, ref bitPosition, 7, Vehicle);

            RtkRecordDataModelHelper.SetLatitude32Bit(buffer, ref bitPosition, Latitude);
            RtkRecordDataModelHelper.SetLongitude32Bit(buffer, ref bitPosition, Longitude);
            RtkRecordDataModelHelper.SetAltitude24Bit(buffer, ref bitPosition, Altitude);

            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, (byte?)Observations);
            RtkRecordDataModelHelper.SetGnssDop10Bit(buffer, ref bitPosition, PDOP);

            RtkRecordDataModelHelper.SetTimeMode2Bit(buffer, ref bitPosition, TimeMode);
            RtkRecordDataModelHelper.SetFixType3Bit(buffer, ref bitPosition, FixType);

            RtkRecordDataModelHelper.SetBoolean2Bit(buffer, ref bitPosition, Active);
            RtkRecordDataModelHelper.SetBoolean2Bit(buffer, ref bitPosition, Valid); bitPosition += 6;

            RtkRecordDataModelHelper.SetAccuracy32Bit(buffer, ref bitPosition, Accuracy);
            RtkRecordDataModelHelper.SetDuration24Bit(buffer, ref bitPosition, Duration);

            SpanBitHelper.SetBitU(buffer, ref bitPosition, 16, RtkSpeedBps);
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
        public ushort? Noise { get; set; }
        public double AgcMonitor { get; set; } = double.NaN;
        public double JammingIndicator { get; set; } = double.NaN;

        public override int GetBitSize()
        {
            return 104;
        }

        public override void Deserialize(ReadOnlySpan<byte> buffer, ref int bitPosition)
        {
            Gps = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            Sbas = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            Glonass = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            BeiDou = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            Galileo = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            Imes = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition);
            Qzss = RtkRecordDataModelHelper.GetObservationCount7Bit(buffer, ref bitPosition); bitPosition += 7;
            Noise = RtkRecordDataModelHelper.GetUShort16Bit(buffer, ref bitPosition);
            AgcMonitor = RtkRecordDataModelHelper.GetAgcJam14Bit(buffer, ref bitPosition);
            JammingIndicator = RtkRecordDataModelHelper.GetAgcJam14Bit(buffer, ref bitPosition); bitPosition += 4;
        }

        public override void Serialize(Span<byte> buffer, ref int bitPosition)
        {
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, Gps);
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, Sbas);
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, Glonass);
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, BeiDou);
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, Galileo);
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, Imes);
            RtkRecordDataModelHelper.SetObservationCount7Bit(buffer, ref bitPosition, Qzss); bitPosition += 7;
            RtkRecordDataModelHelper.SetUShort16Bit(buffer, ref bitPosition, Noise);
            RtkRecordDataModelHelper.SetAgcJam14Bit(buffer, ref bitPosition, AgcMonitor);
            RtkRecordDataModelHelper.SetAgcJam14Bit(buffer, ref bitPosition, JammingIndicator); bitPosition += 4;
        }
    }

    public enum TimeModeEnum
    {
        Unknown,
        Standalone,
        SurveyIn,
        FixedBase
    }

    public enum GnssFixType : byte
    {
        Unknown,
        NoFix,
        Fix2D,
        Fix3D,
        Time
    }

    public static class RtkRecordDataModelHelper
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

        public static double GetLatitude32Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return SpanBitHelper.GetFixedPointS32Bit(buffer, ref bitIndex, ArcSecond, 0, MaxLat, MinLat);
        }

        public static void SetLatitude32Bit(Span<byte> buffer, ref int bitIndex, double latitude)
        {
            SpanBitHelper.SetFixedPointS32Bit(buffer, ref bitIndex, latitude, ArcSecond, 0, MaxLat, MinLat);
        }

        public static double GetLongitude32Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return SpanBitHelper.GetFixedPointS32Bit(buffer, ref bitIndex, ArcSecond, 0, MaxLon, MinLon);
        }

        public static void SetLongitude32Bit(Span<byte> buffer, ref int bitIndex, double longitude)
        {
            SpanBitHelper.SetFixedPointS32Bit(buffer, ref bitIndex, longitude, ArcSecond, 0, MaxLon, MinLon);
        }

        #endregion

        #region GNSS Altitude

        private const double AltFraction = 0.01f;
        public const double MaxAlt = SpanBitHelper.FixedPointS24Max * AltFraction;
        public const double MinAlt = SpanBitHelper.FixedPointS24Min * AltFraction;
        public const uint GnssAltitudeBitSize = 24;

        public static double GetAltitude24Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return SpanBitHelper.GetFixedPointS24Bit(buffer, ref bitIndex, AltFraction);
        }

        public static void SetAltitude24Bit(Span<byte> buffer, ref int bitIndex, double altitude)
        {
            SpanBitHelper.SetFixedPointS24Bit(buffer, ref bitIndex, altitude, AltFraction);
        }

        #endregion

        #region Agc/Jamming

        private const double AgcJamFraction = 0.0001;
        public const double MaxAgcJam = 2 * 5000.0 * AgcJamFraction;
        public const double MinAgcJam = 0.0;
        public const double AgcJamOffset = 5000.0 * AgcJamFraction;
        public static double GetAgcJam14Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return SpanBitHelper.GetFixedPointS14Bit(buffer, ref bitIndex, AgcJamFraction, AgcJamOffset, MaxAgcJam, 0.0);
        }

        public static void SetAgcJam14Bit(Span<byte> buffer, ref int bitIndex, double agcJam)
        {
            SpanBitHelper.SetFixedPointS14Bit(buffer, ref bitIndex, agcJam, AgcJamFraction, AgcJamOffset, MaxAgcJam, 0.0);
        }

        #endregion

        #region Accuracy

        private const double AccFraction = 0.0001;
        public const double MaxAcc = 2.0 * SpanBitHelper.FixedPointS32Max * AccFraction;
        public const double MinAcc = 0.0;
        public const double AccOffset = SpanBitHelper.FixedPointS32Max * AccFraction;

        public static double GetAccuracy32Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return SpanBitHelper.GetFixedPointS32Bit(buffer, ref bitIndex, AccFraction, AccOffset, MaxAcc, MinAcc);
        }

        public static void SetAccuracy32Bit(Span<byte> buffer, ref int bitIndex, double accuracy)
        {
            SpanBitHelper.SetFixedPointS32Bit(buffer, ref bitIndex, accuracy, AccFraction, AccOffset, MaxAcc, MinAcc);
        }

        #endregion

        #region Duration

        public const uint DurNull = 0xFFFFFF;
        public const uint MaxDur = 0xFFFFFE;
        public const int DurationBitSize = 24;

        public static uint? GetDuration24Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            var result = SpanBitHelper.GetBitU(buffer, ref bitIndex, DurationBitSize);
            return result == DurNull ? null : result;
        }

        public static void SetDuration24Bit(Span<byte> buffer, ref int bitIndex, uint? duration)
        {
            duration = duration switch
            {
                null => DurNull,
                > MaxDur => MaxDur,
                _ => duration
            };
            SpanBitHelper.SetBitU(buffer, ref bitIndex, DurationBitSize, duration.Value);
        }

        #endregion

        #region GNSS HDOP\VDOP\PDOP

        public const double DopFraction = 0.1;
        public const double MaxDop = 2.0 * SpanBitHelper.FixedPointS10Max * DopFraction;
        public const double MinDop = 0;
        public const double DopOffset = SpanBitHelper.FixedPointS10Max * DopFraction;
        public const uint DopBitSize = 7;

        public static double GetGnssDop10Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return SpanBitHelper.GetFixedPointS10Bit(buffer, ref bitIndex, DopFraction, DopOffset, MaxDop, MinDop);
        }

        public static void SetGnssDop10Bit(Span<byte> buffer, ref int bitIndex, double dop)
        {
            if (dop < 0) throw new ArgumentOutOfRangeException(nameof(dop));
                        SpanBitHelper.SetFixedPointS10Bit(buffer, ref bitIndex, dop, DopFraction, DopOffset, MaxDop, MinDop);
        }
        #endregion

        #region Observation Count

        public const byte ObservationCountNull = 0x7F;
        public const byte MaxObservationCount = 0x7E;
        public const int ObservationCountBitSize = 7;

        public static byte? GetObservationCount7Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            var result = (byte)SpanBitHelper.GetBitU(buffer, ref bitIndex, ObservationCountBitSize);
            return result == ObservationCountNull ? null : result;
        }

        public static void SetObservationCount7Bit(Span<byte> buffer, ref int bitIndex, byte? count)
        {
            count = count switch
            {
                null => ObservationCountNull,
                > MaxObservationCount => MaxObservationCount,
                _ => count
            };
            SpanBitHelper.SetBitU(buffer, ref bitIndex, ObservationCountBitSize, count.Value);
        }

        #endregion

        #region Time mode

        public const int TimeModeBitSize = 2;

        public static TimeModeEnum GetTimeMode2Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return (TimeModeEnum)SpanBitHelper.GetBitU(buffer, ref bitIndex, TimeModeBitSize);
        }

        public static void SetTimeMode2Bit(Span<byte> buffer, ref int bitIndex, TimeModeEnum mode)
        {
            SpanBitHelper.SetBitU(buffer, ref bitIndex, TimeModeBitSize, (uint)mode);
        }

        #endregion

        #region GNSS Fix type

        public const int FixTypeBitSize = 3;

        public static GnssFixType GetFixType3Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            return (GnssFixType)SpanBitHelper.GetBitU(buffer, ref bitIndex, FixTypeBitSize);
        }

        public static void SetFixType3Bit(Span<byte> buffer, ref int bitIndex, GnssFixType type)
        {
            SpanBitHelper.SetBitU(buffer, ref bitIndex, FixTypeBitSize, (uint)type);
        }

        #endregion

        #region UShort16Bit?

        public const ushort UShortNull = 0xFFFF;
        public const int UShortBitSize = 16;

        public static ushort? GetUShort16Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            var result = (ushort)SpanBitHelper.GetBitU(buffer, ref bitIndex, UShortBitSize);
            if (result == UShortNull) return null;
            return result;
        }

        public static void SetUShort16Bit(Span<byte> buffer, ref int bitIndex, ushort? value)
        {
            value ??= UShortNull;
            SpanBitHelper.SetBitU(buffer, ref bitIndex, UShortBitSize, value.Value);
        }

        #endregion

        #region Bool?

        public const uint BooleanNull = 0x3;
        public const int BooleanBitSize = 2;

        public static bool? GetBoolean2Bit(ReadOnlySpan<byte> buffer, ref int bitIndex)
        {
            var result = SpanBitHelper.GetBitU(buffer, ref bitIndex, BooleanBitSize);
            if (result == BooleanNull) return null;
            return result != 0;
        }

        public static void SetBoolean2Bit(Span<byte> buffer, ref int bitIndex, bool? value)
        {
            var v = value == null ? BooleanNull : value.Value ? 1u : 0;
            SpanBitHelper.SetBitU(buffer, ref bitIndex, BooleanBitSize, v);
        }

        #endregion
    }
}