namespace Asv.Gnss
{
    public static class SbfMessageFactory
    {
        public static SbfBinaryParser RegisterDefaultFrames(this SbfBinaryParser src)
        {
            src.Register(() => new SbfPacketGpsRawCa());
            src.Register(() => new SbfPacketGloRawCa());
            //src.Register(() => new SbfPacketMeasEpoch()); // TODO: not complete
            src.Register(()=> new SbfPacketPvtGeodeticRev2());
            src.Register(()=>new SbfPacketDOP());
            src.Register(()=> new SbfPacketReceiverStatusRev1());
            src.Register(() => new SbfPacketQualityInd());
            return src;
        }
    }
}