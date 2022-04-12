using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;

namespace Asv.Gnss
{
    public interface IUbxDevice
    {
        IObservable<UbxMessageBase> OnUbx { get; }
        IObservable<RtcmV3MessageBase> OnRtcm { get; }

        /// <summary>
        /// Местоположение точки относимости антенны реф. станции RTK
        /// </summary>
        IRxValue<GeoPoint> OnLocation { get; }
        IRxValue<UbxNavSurveyIn> OnSurveyIn { get; }
        IRxValue<UbxNavPvt> OnMovingBase { get; }
        IRxValue<UbxVelocitySolutionInNED> OnVelocitySolution { get; }
        IRxValue<UbxMonitorHardware> OnHwInfo { get; }
        IRxValue<bool> OnReboot { get; }

        IObservable<UbxTimeModeConfiguration> OnUbxTimeMode { get; }
        IObservable<UbxMonitorVersion> OnVersion { get; }
        IObservable<UbxInfWarning> UbxWarning { get; }

        #region TimeMode

        Task StopSurveyInTimeMode(CancellationToken cancel);
        Task SetSurveyInTimeMode(uint duration, double accLimit, CancellationToken cancel);
        Task SetFixedBaseTimeMode(GeoPoint location, double accLimit, CancellationToken cancel);
        Task SetStandaloneTimeMode(CancellationToken cancel);



        #endregion

        Task SetRtcmRate(byte msgRate, CancellationToken cancel);

        Task SetupByDefault(CancellationToken cancel);

        Task<UbxNavSatellite> GetUbxNavSat(CancellationToken cancel = default);
        Task<UbxNavPvt> GetUbxNavPvt(CancellationToken cancel = default);
        Task<UbxMonitorVersion> GetUbxMonVer(CancellationToken cancel = default);
    }
}
