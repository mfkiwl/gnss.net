using System;
using System.Collections.Generic;

namespace Asv.Gnss
{
    public class SbfBinaryParser: GnssParserBase
    {
        public const string GnssProtocolId = "SBF";
        public const int MaxPacketSize = 1024 * 4;
        private State _state;
        private readonly byte[] _buffer = new byte[MaxPacketSize];
        private int _bufferIndex = 0;
        private byte _headerLength;
        private ushort _messageLength;
        private int _stopMessageIndex;
        private IDiagnosticSource _diag;
        public override string ProtocolId => GnssProtocolId;
        private readonly Dictionary<ushort, Func<ComNavBinaryPacketBase>> _dict = new Dictionary<ushort, Func<ComNavBinaryPacketBase>>();

        public SbfBinaryParser(IDiagnostic diag)
        {
            _diag = diag[GnssProtocolId];
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
                    if (data != 0x24) return false;
                    _bufferIndex = 0;
                    _buffer[_bufferIndex++] = 0x24;
                    _state = State.Sync2;
                    break;
                case State.Sync2:
                    if (data != 0x40)
                    {
                        _state = State.Sync1;
                    }
                    else
                    {
                        _state = State.Sync3;
                        _buffer[_bufferIndex++] = 0x40;
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
                            ParsePacket(_buffer);
                        }
                        else
                        {
                            _diag.Int["crc err"]++;
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

        public void Register(Func<ComNavBinaryPacketBase> factory)
        {
            var testPckt = factory();
            _dict.Add(testPckt.MessageId, factory);
        }

        private void ParsePacket(byte[] data)
        {
            var msgId =  BitConverter.ToUInt16(data, 4);
            _diag.Speed[msgId.ToString()].Increment(1);
            if (_dict.TryGetValue(msgId, out var factory) == false)
            {
                _diag.Int["unk err"]++;
                InternalOnError(new GnssParserException(ProtocolId, $"Unknown ComNavBinary packet message number [MSG={msgId}]"));
                return;
            }

            var message = factory();

            try
            {
                message.Deserialize(data);
            }
            catch (Exception e)
            {
                _diag.Int["parse err"]++;
                InternalOnError(new GnssParserException(ProtocolId, $"Parse ComNavBinary packet error [MSG={msgId}]", e));
            }

            try
            {
                InternalOnMessage(message);
            }
            catch (Exception e)
            {
                _diag.Int["pub err"]++;
                InternalOnError(new GnssParserException(ProtocolId, $"Parse ComNavBinary packet error [MSG={msgId}]", e));
            }
        }

        public override void Reset()
        {
            _state = State.Sync1;
        }
    }
}