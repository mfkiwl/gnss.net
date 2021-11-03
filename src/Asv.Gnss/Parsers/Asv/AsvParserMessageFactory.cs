namespace Asv.Gnss
{
    public static class AsvParserMessageFactory
    {
        public static AsvParser RegisterDefaultFrames(this AsvParser src)
        {
            src.Register(() => new AsvMessageGbasVdbSend());
            src.Register(() => new AsvMessageHeartBeat());
            return src;
        }
    }
}