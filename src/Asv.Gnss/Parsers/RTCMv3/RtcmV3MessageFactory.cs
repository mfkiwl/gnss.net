﻿namespace Asv.Gnss
{
    public static class RtcmV3MessageFactory
    {
        public static RtcmV3Parser RegisterDefaultFrames(this RtcmV3Parser src)
        {
            src.Register(() => new RtcmV3MSM4(1074));
            src.Register(() => new RtcmV3MSM4(1084));
            src.Register(() => new RtcmV3MSM4(1094));
            src.Register(() => new RtcmV3MSM4(1124));
            src.Register(() => new RtcmV3MSM7(1077));
            src.Register(() => new RtcmV3MSM7(1087));
            src.Register(() => new RtcmV3MSM7(1097));
            src.Register(() => new RtcmV3MSM7(1127));
            src.Register(() => new RtcmV3Message1005());
            src.Register(() => new RtcmV3Message1006());
            src.Register(() => new RtcmV3Message1019());
            src.Register(() => new RtcmV3Message1020());
            return src;
        }
    }
}