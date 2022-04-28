using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;

namespace Asv.Gnss
{
    public interface IUbxDevice : IRtkDevice
    {
        IObservable<UbxMessageBase> OnUbx { get; }
        IObservable<RtcmV3MessageBase> OnRtcm { get; }

        IRxValue<GeoPoint> OnLocation { get; }
        IRxValue<UbxNavSurveyIn> OnSurveyIn { get; }
        IRxValue<UbxNavPvt> OnMovingBase { get; }
        IRxValue<UbxVelocitySolutionInNED> OnVelocitySolution { get; }
        IRxValue<UbxMonitorHardware> OnHwInfo { get; }
        
        IObservable<UbxTimeModeConfiguration> OnUbxTimeMode { get; }
        IObservable<UbxMonitorVersion> OnVersion { get; }
        IObservable<UbxInfWarning> UbxWarning { get; }

        Task SetupByDefault(CancellationToken cancel);

        Task<UbxTimeModeConfiguration> GetUbxTimeMode(CancellationToken cancel = default);
        Task<UbxNavSatellite> GetUbxNavSat(CancellationToken cancel = default);
        Task<UbxNavPvt> GetUbxNavPvt(CancellationToken cancel = default);
        Task<UbxMonitorVersion> GetUbxMonVersion(CancellationToken cancel = default);
    }
}
