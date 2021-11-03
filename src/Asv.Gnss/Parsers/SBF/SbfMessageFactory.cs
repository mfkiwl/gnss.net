namespace Asv.Gnss
{
    public static class SbfMessageFactory
    {
        public static SbfBinaryParser RegisterDefaultFrames(this SbfBinaryParser src)
        {
            src.Register(() => new SbfPacketGpsRawCa());
            return src;
        }
    }
}