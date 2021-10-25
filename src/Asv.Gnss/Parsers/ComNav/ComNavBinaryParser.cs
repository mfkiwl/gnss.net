using System;
using System.Collections.Generic;

namespace Asv.Gnss
{
    public class ComNavBinaryParser: GnssParserWithMessagesBase<ComNavBinaryPacketBase,ushort>
    {
        public const string GnssProtocolId = "BinaryComNav";
        public const int MaxPacketSize = 1024 * 4;
        private State _state;
        private readonly byte[] _buffer = new byte[MaxPacketSize];
        private int _bufferIndex = 0;
        private byte _headerLength;
        private ushort _messageLength;
        private int _stopMessageIndex;
        public override string ProtocolId => GnssProtocolId;

        public ComNavBinaryParser(IDiagnostic diag):base(diag)
        {
        }

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
                    if (_bufferIndex == _stopMessageIndex)
                    {
                         /* step back to last byte */
                        var crc32Index = _bufferIndex - 4 /* CRC32 */;
                        var calculatedHash = ComNavCrc32.ComputeChecksum(_buffer, 0, crc32Index);
                        var readedHash = BitConverter.ToUInt32(_buffer, crc32Index);
                        if (calculatedHash == readedHash)
                        {
                            var msgId = BitConverter.ToUInt16(_buffer, 4);
                            ParsePacket(msgId,_buffer);
                        }
                        else
                        {
                            Diag.Int["crc err"]++;
                            InternalOnError(new GnssParserException(ProtocolId, $"ComNav crc32 error"));
                        }
                        _state = State.Sync1;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();

            }
            return false;
        }

        public override void Reset()
        {
            _state = State.Sync1;
        }
    }
}