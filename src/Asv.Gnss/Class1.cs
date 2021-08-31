using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asv.Gnss
{

    public interface IGnssParser
    {
        bool Read(byte data);
        void Reset();
    }

    public enum RtcmV3ParserState
    {
        S1_Sync,

    }

    public class RtcmV3Parser:IGnssParser
    {
        private const byte Rtcm3Preamb = 0xD3;

        public byte[] _buffer { get; } = new byte[1024 * 4];

        private RtcmV3ParserState _state;

        public bool Read(byte data)
        {
            switch (_state)
            {
                case RtcmV3ParserState.S1_Sync:
                    if (data == Rtcm3Preamb)
                    {
                        _state = 1;
                        _buffer[0] = data;
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
