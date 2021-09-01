﻿using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace Asv.Gnss
{
    public class RtcmV3Parser:IGnssParser
    {
        public RtcmV3Parser()
        {
            
        }

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
        private ushort _payloadReadedBytes;
        private ushort _payloadLength;
        private readonly Subject<GnssParserException> _onErrorSubject = new Subject<GnssParserException>();
        private readonly Dictionary<ushort, Func<RtcmV3MessageBase>> _dict = new Dictionary<ushort, Func<RtcmV3MessageBase>>();
        private int _unknownMessageId;
        private int _crcErrors;
        private int _deserializePacketError;

        public string ProtocolId => "RTCMv3";

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
                    _payloadLength = RtcmV3Helper.GetRtcmV3PacketLength(_buffer);
                    _payloadReadedBytes = 0;
                    if (_payloadLength > _buffer.Length)
                    {
                        // buffer oversize
                        Reset();
                    }
                    break;
                case State.Payload:
                    // read payload
                    _buffer[3 + _payloadReadedBytes] = data;
                    ++_payloadReadedBytes;
                    if (_payloadReadedBytes == _payloadLength)
                    {
                        _state = State.Crc1;
                    }
                    break;
                case State.Crc1:
                    _buffer[_payloadReadedBytes + 3] = data;
                    ++_payloadReadedBytes;
                    _state = State.Crc2;
                    break;
                case State.Crc2:
                    _buffer[_payloadReadedBytes + 3] = data;
                    ++_payloadReadedBytes;
                    _state = State.Crc3;
                    break;
                case State.Crc3:
                    _buffer[_payloadReadedBytes + 3] = data;
                    
                    var originalCrc = RtcmV3Helper.CalculateCrc(_buffer, _payloadLength);
                    var sourceCrc = RtcmV3Helper.ReadCrc(_buffer, _payloadLength);
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

        public void Dispose()
        {
            _onErrorSubject?.Dispose();
        }
    }



    public static class CommonHelper
    {
        public static void RegisterDefaultFrames(this RtcmV3Parser src)
        {
            src.Register();
        }
    }
}