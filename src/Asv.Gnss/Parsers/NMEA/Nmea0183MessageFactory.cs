namespace Asv.Gnss
{
    public static class Nmea0183MessageFactory
    {
        public static Nmea0183Parser RegisterDefaultFrames(this Nmea0183Parser src)
        {
            src.Register(() => new Nmea0183MessageGGA());
            src.Register(() => new Nmea0183MessageGLL());
            src.Register(() => new Nmea0183MessageGSA());
            src.Register(() => new Nmea0183MessageGST());
            src.Register(() => new Nmea0183MessageGSV());
            return src;
        }
    }
}