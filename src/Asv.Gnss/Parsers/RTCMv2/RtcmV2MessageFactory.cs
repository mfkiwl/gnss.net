namespace Asv.Gnss
{
    public static class RtcmV2MessageFactory
    {
        public static RtcmV2Parser RegisterDefaultFrames(this RtcmV2Parser src)
        {
            src.Register(() => new RtcmV2Message1());
            src.Register(() => new RtcmV2Message31());
            src.Register(() => new RtcmV2Message14());
            src.Register(() => new RtcmV2Message15());
            src.Register(() => new RtcmV2Message17());
            return src;
        }
    }
}