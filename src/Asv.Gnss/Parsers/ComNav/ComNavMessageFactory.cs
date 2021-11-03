namespace Asv.Gnss
{
    public static class ComNavMessageFactory
    {
        public static ComNavBinaryParser RegisterDefaultFrames(this ComNavBinaryParser src)
        {
            src.Register(() => new ComNavBinaryPsrPosPacket());
            return src;
        }
    }
}