using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Tools;

namespace Asv.Gnss
{
    public interface IUbxDevice : IRtkStation
    {
        IObservable<UbxMessageBase> OnUbx { get; }
        IObservable<UbxAck> OnAck { get; }
        IObservable<UbxNak> OnNak { get; }

        /// <summary>
        /// Местоположение точки относимости антенны реф. станции RTK
        /// </summary>
        IRxValue<GeoPoint> OnMovingBasePosition { get; }
        IRxValue<UbxNavSurveyIn> OnSurveyIn { get; }
        IRxValue<UbxNavPvt> OnMovingBase { get; }
        IRxValue<UbxMonitorHardware> OnHwInfo { get; }
        IRxValue<UbxMonitorVersion> OnVersion { get; }

        Task SetupByDefault(CancellationToken cancel);
        Task StopSurveyIn(CancellationToken cancel);
        Task SetTMode3SurveyIn(uint duration, double accLimit, CancellationToken cancel);
        Task SetTMode3FixedBase(GeoPoint location, double accLimit, CancellationToken cancel);
        Task SetTMode3MovingBase(CancellationToken cancel);

        Task SetStationaryMode(bool movingBase, CancellationToken cancel);
        Task TurnOffNMEA(CancellationToken cancel);
        Task SetMessageRate(byte msgClass, byte msgSubClass, byte msgRate, CancellationToken cancel);
        Task PollMsg(byte msgClass, byte msgSubClass, CancellationToken cancel);

        Task<UbxNavSatellite> GetUbxNavSat(CancellationToken cancel = default);
        Task<UbxNavPvt> GetUbxNavPvt(CancellationToken cancel = default);
        Task<UbxMonitorVersion> GetUbxMonVer(CancellationToken cancel = default);
    }
}
