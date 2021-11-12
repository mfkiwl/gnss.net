using System;
using System.Threading;

namespace Asv.Gnss
{
    public interface ITimeService
    {
        void SetCorrection(long correctionIn100NanosecondsTicks);
        DateTime Now { get; }
    }

    public class DefaultTimeService : ITimeService
    {
        private static readonly object _sync = new object();
        private static DefaultTimeService _default;

        public static DefaultTimeService Default
        {
            get
            {
                if (_default == null)
                {
                    lock (_sync)
                    {
                        if (_default == null) _default = new DefaultTimeService();
                    }
                }
                return _default;
            }
        }


        public void SetCorrection(long correctionIn100NanosecondsTicks)
        {
            throw new NotImplementedException();
        }

        public DateTime Now => DateTime.Now;
    }

    public class CorrectedTimeService : ITimeService
    {
        private long _correction;

        public void SetCorrection(long correctionIn100NanosecondsTicks)
        {
            Interlocked.Exchange(ref _correction, correctionIn100NanosecondsTicks);

        }

        public DateTime Now => DateTime.Now.AddTicks(Interlocked.Read(ref _correction));
    }
}