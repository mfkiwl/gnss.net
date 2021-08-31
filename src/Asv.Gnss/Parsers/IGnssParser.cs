namespace Asv.Gnss
{
    public interface IGnssParser
    {
        bool Read(byte data);
        void Reset();
    }
}