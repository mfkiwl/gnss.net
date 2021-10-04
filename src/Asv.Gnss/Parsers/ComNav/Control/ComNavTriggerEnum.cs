namespace Asv.Gnss.Control
{
    /// <summary>
    /// Log Trigger Types
    /// </summary>
    public enum ComNavTriggerEnum
    {
        /// <summary>
        /// Synch
        /// </summary>
        ONTIME,
        /// <summary>
        /// Asynch
        /// Logs Supporting ONCHANGED and ONTRACKED
        /// 1 8 IONUTC 4.2.9.1
        /// 2 41 RAWEPHEM 4.2.1.17
        /// 3 71 BD2EPHEM 4.2.1.1
        /// 4 79 BINEX0101 4.3.4.2
        /// 5 80 BINEX0102 4.3.4.2
        /// 6 84 BINEX0105 4.3.4.2
        /// 7 89 RTCM0063 4.3.3.1
        /// 8 90 RTCM4011 NA
        /// 9 104 RTCM4013 NA
        /// 10 175 REFSTATION 4.2.11.1
        /// 11 412 BD2RAWEPHEM 4.2.1.5
        /// 12 712 GPSEPHEM 4.2.1.9
        /// 13 723 GLOEPHEMERIS 4.2.1.4
        /// 14 792 GLORAWEPHEM 4.2.1.8
        /// 15 893 RTCM1019 4.3.3.12
        /// 16 895 RTCM1020 4.3.3.13
        /// </summary>
        ONCHANGED,
        /// <summary>
        /// Polled
        /// </summary>
        ONCE
    }
}