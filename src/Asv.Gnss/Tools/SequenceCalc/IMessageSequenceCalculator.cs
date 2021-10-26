namespace Asv.Gnss
{
    public interface IMessageSequenceCalculator
    {
        ushort GetNextSequenceNumber();
    }
}