using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Asv.Gnss
{
    public class RtcmV3Parser:IGnssParser
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
        private readonly Subject<GnssParserException> _onErrorSubject = new Subject<GnssParserException>();
        private readonly Subject<GnssMessageBase> _onMessage = new Subject<GnssMessageBase>();
        private readonly Dictionary<ushort, Func<RtcmV3MessageBase>> _dict = new Dictionary<ushort, Func<RtcmV3MessageBase>>();
        private int _unknownMessageId;
        private int _crcErrors;
        private int _deserializePacketError;

        public string ProtocolId => GnssProtocolId;

        public RtcmV3Parser()
        {
            Register(() => new RtcmV3MSM4(1074));
            Register(() => new RtcmV3MSM4(1084));
            Register(() => new RtcmV3MSM4(1094));
            Register(() => new RtcmV3MSM4(1124));
        }

        public bool Read(byte data)
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
                        ParsePacket(_buffer);
                    }
                    else
                    {
                        _crcErrors++;
                        _onErrorSubject.OnNext(new GnssParserException(ProtocolId,$"Crc error [total={_crcErrors}]"));
                    }
                    Reset();
                    return true;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        public void Register(Func<RtcmV3MessageBase> factory)
        {
            var testPckt = factory();
            _dict.Add(testPckt.MessageId, factory);
        }

        private void ParsePacket(byte[] data)
        {
            var msgNumber = RtcmV3Helper.ReadMessageNumber(data);
            if (_dict.TryGetValue(msgNumber, out var factory) == false)
            {
                _onErrorSubject.OnNext(new GnssParserException(ProtocolId, $"Unknown RTCMv3 packet message number [MSG={msgNumber}]"));
                ++_unknownMessageId;
                return;
            }

            var message = factory();
                
            try
            {
                message.Deserialize(data,0);
            }
            catch (Exception)
            {
                _deserializePacketError++;
                _onErrorSubject.OnNext(new GnssParserException(ProtocolId, $"Parse RTCMv3 packet error [MSG={msgNumber}]"));
            }
        }

        public void Reset()
        {
            _state = State.Sync;
        }

        public IObservable<GnssParserException> OnError => _onErrorSubject;
        public IObservable<GnssMessageBase> OnMessage => _onMessage;

        public void Dispose()
        {
            _onErrorSubject.Dispose();
            _onMessage.Dispose();
        }
    };



    public static class CommonHelper
    {
        public static RtcmV3Parser RegisterDefaultFrames(this RtcmV3Parser src)
        {
            return src;
        }
    }
}