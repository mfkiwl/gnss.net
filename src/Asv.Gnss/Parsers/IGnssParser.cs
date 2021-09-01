namespace Asv.Gnss
{
    public interface IGnssParser
    {
        string ProtocolId { get; }
        bool Read(byte data);
        void Reset();
    }


    
}