﻿using System;
using System.Collections.Generic;

namespace Asv.Gnss
{
    public class RtcmV2Parser : GnssParserBase
    {
        private const byte SyncByte = 0x66;

        private readonly Dictionary<ushort, Func<RtcmV2MessageBase>> _dict = new Dictionary<ushort, Func<RtcmV2MessageBase>>();
        private readonly IDiagnosticSource _diag;
        
        public const string GnssProtocolId = "RTCMv2";

        private readonly byte[] _buffer = new byte[33 * 3]; /* message buffer   */
        private uint _word;                /* word buffer for rtcm 2            */
        private int _readedBytes;          /* number of bytes in message buffer */
        private int _readedBits;           /* number of bits in word buffer     */
        private int _len;                  /* message length (bytes)            */

        private bool DecodeWord(uint word, byte[] data, int offset)
        {
            var hamming = new uint[] { 0xBB1F3480, 0x5D8F9A40, 0xAEC7CD00, 0x5763E680, 0x6BB1F340, 0x8B7A89C0 };
            uint parity = 0;
            
            if ((word & 0x40000000) != 0) word ^= 0x3FFFFFC0;

            for (var i = 0; i < 6; i++)
            {
                parity <<= 1;
                for (var w = (word & hamming[i]) >> 6; w != 0; w >>= 1) 
                    parity ^= w & 1;
            }
            if (parity != (word & 0x3F)) return false;

            for (var i = 0; i < 3; i++) data[i + offset] = (byte)(word >> (22 - i * 8));
            return true;
        }

        public override string ProtocolId => GnssProtocolId;

        public RtcmV2Parser(IDiagnostic diag)
        {
            _diag = diag[GnssProtocolId];
        }

        public override bool Read(byte data)
        {
            if ((data & 0xC0) != 0x40) return false; /* ignore if upper 2bit != 01 */

            for (var i = 0; i < 6; i++, data >>= 1)
            {
                /* decode 6-of-8 form */
                _word = (uint)((_word << 1) + (data & 1));

                /* synchronize frame */
                if (_readedBytes == 0)
                {
                    var preamb = (byte)(_word >> 22);
                    if ((_word & 0x40000000) != 0) preamb ^= 0xFF; /* decode preamble */
                    if (preamb != SyncByte) continue;

                    /* check parity */
                    if (!DecodeWord(_word, _buffer, 0)) continue;
                    _readedBytes = 3; _readedBits = 0;
                    continue;
                }
                if (++_readedBits < 30) continue;
                _readedBits = 0;
                
                /* check parity */
                if (!DecodeWord(_word, _buffer, _readedBytes))
                {
                    _diag.Int["crc err"]++;
                    InternalOnError(new GnssParserException(ProtocolId, $"RTCMv2 crc error"));
                    _readedBytes = 0; _word &= 0x3;
                    continue;
                }
                _readedBytes += 3;
                if (_readedBytes == 6) _len = (_buffer[5] >> 3) * 3 + 6;
                if (_readedBytes < _len) continue;
                _readedBytes = 0;
                _word &= 0x3;

                /* decode rtcm2 message */
                ParsePacket(_buffer);
                Reset();
                return true;
            }
            return false;

        }

        public void Register(Func<RtcmV2MessageBase> factory)
        {
            var testPckt = factory();
            _dict.Add(testPckt.MessageId, factory);
        }

        private void ParsePacket(byte[] data)
        {
            var msgType = (ushort)RtcmV3Helper.GetBitU(data, 8, 6);
            _diag.Speed[msgType.ToString()].Increment(1);

            if (_dict.TryGetValue(msgType, out var factory) == false)
            {
                _diag.Int["unk err"]++;
                InternalOnError(new GnssParserException(ProtocolId, $"Unknown RTCMv2 packet message number [MSG={msgType}]"));
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
                InternalOnError(new GnssParserException(ProtocolId, $"Parse RTCMv2 packet error [MSG={msgType}]", e));
            }

            try
            {
                InternalOnMessage(message);
            }
            catch (Exception e)
            {
                _diag.Int["pub err"]++;
                InternalOnError(new GnssParserException(ProtocolId, $"Parse RTCMv2 packet error [MSG={msgType}]", e));
            }

        }

        public override void Reset()
        {
            _word = 0;
            _readedBytes = 0;
            _readedBits = 0;
            _len = 0;
    }
    }
}