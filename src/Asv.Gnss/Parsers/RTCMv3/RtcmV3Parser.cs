using System;

namespace Asv.Gnss
{
    public class RtcmV3Parser:IGnssParser
    {
        private readonly byte[] _buffer = new byte[1024 * 4];

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
        private RtcmV3Preamble _preamble;
        private uint _payloadByteReaded;

        public bool Read(byte data)
        {
            switch (_state)
            {
                case State.Sync:
                    if (data == RtcmV3Preamble.SyncByte)
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
                    _preamble = new RtcmV3Preamble();
                    _preamble.Deserialize(_buffer);
                    _payloadByteReaded = 0;
                    // buffer oversize
                    if (_preamble.PacketLength > _buffer.Length)
                        _state = State.Preamb1;
                    break;
                case State.Payload:
                    // read payload
                    _buffer[3 + _payloadByteReaded] = data;
                    ++_payloadByteReaded;
                    if (_payloadByteReaded >= _preamble.PacketLength)
                    {
                        _state = State.Crc1;
                    }
                    break;
                case State.Crc1:
                    _buffer[_payloadByteReaded + 3] = data;
                    ++_payloadByteReaded;
                    _state = State.Crc2;
                    break;
                case State.Crc2:
                    _buffer[_payloadByteReaded + 3] = data;
                    ++_payloadByteReaded;
                    _state = State.Crc3;
                    break;
                case State.Crc3:
                    _buffer[_payloadByteReaded + 3] = data;
                    _payloadByteReaded += 3;
                    var originalCrc = Crc24.Calc(_buffer, _payloadByteReaded, 0);
                    var sourceCrc = RtcmV3Helper.GetBitU(_buffer, _payloadByteReaded * 8 /*bit in bytes*/,24 /* crc length 3 byte*/);
                    if (originalCrc == sourceCrc)
                    {
                        ParsePacket(_buffer, _preamble);
                    }
                    else
                    {
                        CrcErrors++;
                    }
                    Reset();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public uint CrcErrors { get; private set; }
        public int UnknownMessageId { get; private set; }
        public int DeserializePacketError { get; private set; }

        private void ParsePacket(byte[] data, RtcmV3Preamble preamble)
        {
            var head = new RtcmV3Header();
            head.Deserialize(data);
            RtcmV3MessageBase packet;
            switch (head.MessageNumber)
            {
                case 1002:
                    packet = new RtcmV3Message1002(preamble,head);
                    break;

                default:
                    ++UnknownMessageId;
                    return;
            }

            try
            {
                packet.Deserialize(data);
            }
            catch (Exception)
            {
                DeserializePacketError++;
            }
        }

        public void Reset()
        {
            _state = State.Sync;
        }
    }
}