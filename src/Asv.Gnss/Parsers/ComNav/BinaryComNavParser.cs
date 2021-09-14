using System;

namespace Asv.Gnss
{
    public class BinaryComNavParser: GnssParserBase
    {
        private State _state;
        private readonly byte[] _buffer = new byte[1024*4];
        private int _bufferIndex = 0;
        private byte _headerLength;
        private int _headerRecv;
        private ushort _messageLength;
        private int _stopMessageIndex;
        public override string ProtocolId => "BinaryComNav";

        private enum State
        {
            Sync1,
            Sync2,
            Sync3,
            HeaderLength,
            Header,
            Message
        }

        public override bool Read(byte data)
        {
            switch (_state)
            {
                case State.Sync1:
                    if (data != 0xAA) return false;
                    _bufferIndex = 0;
                    _buffer[_bufferIndex++] = 0xAA;
                    _state = State.Sync2;
                    break;
                case State.Sync2:
                    if (data != 0x44)
                    {
                        _state = State.Sync1;
                    }
                    else
                    {
                        _state = State.Sync3;
                        _buffer[_bufferIndex++] = 0x44;
                    }
                    break;
                case State.Sync3:
                    if (data != 0x12)
                    {
                        _state = State.Sync1;
                    }
                    else
                    {
                        _state = State.HeaderLength;
                        _buffer[_bufferIndex++] = 0x12;
                    }
                    break;
                case State.HeaderLength:
                    _headerLength = data;
                    _buffer[_bufferIndex++] = data;
                    _state = State.Header;
                    break;
                case State.Header:
                    _buffer[_bufferIndex++] = data;
                    if (_bufferIndex == _headerLength)
                    {
                        _messageLength = BitConverter.ToUInt16(_buffer, 8);
                        _stopMessageIndex = _headerLength + _messageLength + 4 /* CRC 32 bit*/;
                        _state = State.Message;
                    }
                    break;
                case State.Message:
                    _buffer[_bufferIndex++] = data;
                    if (_bufferIndex == (_stopMessageIndex))
                    {
                        
                        _state = State.Sync1;
                    }
                    break;

            }
            return false;
        }

        public override void Reset()
        {
            _state = State.Sync1;
        }
    }
}