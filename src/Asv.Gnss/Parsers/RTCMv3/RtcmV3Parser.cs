using System;
using System.Collections.Generic;

namespace Asv.Gnss
{
    public class RtcmV3Parser: GnssParserWithMessagesBase<RtcmV3MessageBase,ushort>
    {
        public const string GnssProtocolId = "RTCMv3";

        private readonly byte[] _buffer = new byte[1024 * 6];

        private enum State
        {
            Sync,
            Preamb1,
            Preamb2,
            Payload,
            Crc2,
            Crc1,
            Crc3
        }

        private State _state;
        private ushort _payloadReadedBytes;
        private uint _payloadLength;

        public RtcmV3Parser(IDiagnostic diag):base(diag)
        {

        }

        public override string ProtocolId => GnssProtocolId;

        public override bool Read(byte data)
        {
            switch (_state)
            {
                case State.Sync:
                    if (data == RtcmV3Helper.SyncByte)
                    {
                        _state = State.Preamb1;
                        _buffer[0] = data;
                    }
                    break;
                case State.Preamb1:
                    _buffer[1] = data;
                    _state = State.Preamb2;
                    break;
                case State.Preamb2:
                    _buffer[2] = data;
                    _state = State.Payload;
                    _payloadLength = RtcmV3Helper.GetRtcmV3PacketLength(_buffer,0);
                    _payloadReadedBytes = 0;
                    if (_payloadLength > _buffer.Length)
                    {
                        // buffer oversize
                        Reset();
                    }
                    break;
                case State.Payload:
                    // read payload
                    if (_payloadReadedBytes < _payloadLength)
                    {
                        _buffer[_payloadReadedBytes + 3] = data;
                        ++_payloadReadedBytes;
                    }
                    else
                    {
                        _state = State.Crc1;
                        goto case State.Crc1;
                    }
                    break;
                case State.Crc1:
                    _buffer[_payloadReadedBytes + 3] = data;
                    _state = State.Crc2;
                    break;
                case State.Crc2:
                    _buffer[_payloadReadedBytes + 3 + 1] = data;
                    _state = State.Crc3;
                    break;
                case State.Crc3:
                    _buffer[_payloadReadedBytes + 3 + 2] = data;
                    _payloadLength = _payloadLength + 3;
                    
                    var originalCrc = Crc24.Calc(_buffer, _payloadLength, 0);
                    var sourceCrc = RtcmV3Helper.GetBitU(_buffer,_payloadLength * 8,24);
                    if (originalCrc == sourceCrc)
                    {
                        var msgNumber = RtcmV3Helper.ReadMessageNumber(_buffer);
                        ParsePacket(msgNumber,_buffer);
                    }
                    else
                    {
                        Diag.Int["crc err"]++;
                        InternalOnError(new GnssParserException(ProtocolId,$"RTCMv3 crc error"));
                    }
                    Reset();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public override void Reset()
        {
            _state = State.Sync;
        }

    };
}