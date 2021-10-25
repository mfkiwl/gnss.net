using System.Threading;

namespace Asv.Gnss
{
    public class MessageSequenceCalculator : IMessageSequenceCalculator
    {
        private volatile int _seq;

        public ushort GetNextSequenceNumber()
        {
            return (ushort)(Interlocked.Increment(ref _seq) % ushort.MaxValue);
        }
    }
}