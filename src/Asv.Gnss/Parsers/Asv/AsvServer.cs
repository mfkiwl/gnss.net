using System;

namespace Asv.Gnss
{
    public class AsvServer:IDisposable
    {
        private readonly GnssConnection _conn;

        public AsvServer(GnssConnection conn)
        {
            _conn = conn;
        }

        

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }

    
}