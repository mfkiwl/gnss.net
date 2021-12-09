using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace Asv.Gnss
{
    public class RtcmV2Parser2 : GnssParserWithMessagesBase<RtcmV2MessageBase, ushort>
    {
        private State _state;
        public const string GnssProtocolId = "RTCMv2";
        private byte[] _buffer = new byte[256];
        private byte[] _data = new byte[256];
        private const byte RTCMv2SyncByte = 0b0110_0110;
        private int _bufferIndex = 0;

        private const uint PARITY_25 = 0xBB1F3480;
        private const uint PARITY_26 = 0x5D8F9A40;
        private const uint PARITY_27 = 0xAEC7CD00;
        private const uint PARITY_28 = 0x5763E680;
        private const uint PARITY_29 = 0x6BB1F340;
        private const uint PARITY_30 = 0x8B7A89C0;

        private static readonly byte[] ByteParity = new byte[] {
            0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,
            1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,
            1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,
            0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,
            1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,
            0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,
            0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0,1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,
            1,0,0,1,0,1,1,0,0,1,1,0,1,0,0,1,0,1,1,0,1,0,0,1,1,0,0,1,0,1,1,0
        };
        private static readonly byte[] Swap = {
            0,32,16,48, 8,40,24,56, 4,36,20,52,12,44,28,60,
            2,34,18,50,10,42,26,58, 6,38,22,54,14,46,30,62,
            1,33,17,49, 9,41,25,57, 5,37,21,53,13,45,29,61,
            3,35,19,51,11,43,27,59, 7,39,23,55,15,47,31,63
        };

        private uint _word;
        private int _numberOfWords;

        public RtcmV2Parser2(IDiagnosticSource diagSource) : base(diagSource)
        {

        }

        public override string ProtocolId => GnssProtocolId;

        private enum State
        {
            Word1,
            Word2,
            Payload
        }
            
        public override bool Read(byte data)
        {
            if ((data & 0x40) != 0x40)
            {
                return false;
            }

            var found = false;
            //data = Swap[data & 0x3f];
            for (int i = 0; i < 6; i++)
            {
                _word = (_word << 1) + (uint)(data & 1);
                if (CheckParity(_word) == true)
                {
                    found = true;
                    break;
                }
            }

            if (found == false) return false;
           
            var dataWord = _word;
            if ((_word & 0x40000000) != 0) dataWord ^= 0x3FFFFFC0;
            switch (_state)
            {
                case State.Word1:
                    if (((byte)(dataWord >> 22) & 0xFF) == RTCMv2SyncByte)
                    {
                        _state = State.Word2;
                        _buffer[0] = (byte)((dataWord >> 22) & 0xFF); ;
                        _buffer[1] = (byte)((dataWord >> 14) & 0xFF);
                        _buffer[2] = (byte)((dataWord >> 6) & 0xFF);
                        
                    }
                    break;
                case State.Word2:
                    _buffer[3] = (byte)((dataWord >> 22) & 0xFF); ;
                    _buffer[4] = (byte)((dataWord >> 14) & 0xFF);
                    _buffer[5] = (byte)((dataWord >> 6) & 0xFF);
                    _bufferIndex = 6;
                    _numberOfWords = _buffer[5] >> 3;
                    if (_numberOfWords == 0)
                    {
                        _state = State.Word1;
                    }
                    else
                    {
                        _state = State.Payload;
                    }
                    break;
                case State.Payload:
                    _buffer[_bufferIndex++] = (byte)((dataWord >> 22) & 0xFF); ;
                    _buffer[_bufferIndex++] = (byte)((dataWord >> 14) & 0xFF);
                    _buffer[_bufferIndex++] = (byte)((dataWord >> 6) & 0xFF);
                    --_numberOfWords;
                    if (_numberOfWords <= 0)
                    {
                        var msgId = _buffer[1] >> 2;
                        _state = State.Word1;
                        ParsePacket((ushort) msgId,_buffer);

                        return true;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        private static bool CheckParity(uint word)
        {
            // Local variables
            uint t, w, p;

            // The sign of the data is determined by the D30* parity bit 
            // of the previous data word. If  D30* is set, invert the data 
            // bits D01..D24 to obtain the d01..d24 (but leave all other
            // bits untouched).

            w = word;
            if ((w & 0x40000000) != 0) w ^= 0x3FFFFFC0;

            // Compute the parity of the sign corrected data bits d01..d24
            // as described in the ICD-GPS-200

            t = w & PARITY_25;
            p = (uint) (ByteParity[t & 0xff] ^ ByteParity[(t >> 8) & 0xff] ^ ByteParity[(t >> 16) & 0xff] ^ ByteParity[(t >> 24)]);

            t = w & PARITY_26;
            p = (p << 1) | (uint)(ByteParity[t & 0xff] ^ ByteParity[(t >> 8) & 0xff] ^ ByteParity[(t >> 16) & 0xff] ^ ByteParity[(t >> 24)]);

            t = w & PARITY_27;
            p = (p << 1) | (uint)(ByteParity[t & 0xff] ^ ByteParity[(t >> 8) & 0xff] ^  ByteParity[(t >> 16) & 0xff] ^ ByteParity[(t >> 24)]);

            t = w & PARITY_28;
            p = (p << 1) | (uint)(ByteParity[t & 0xff] ^ ByteParity[(t >> 8) & 0xff] ^  ByteParity[(t >> 16) & 0xff] ^ ByteParity[(t >> 24)]);

            t = w & PARITY_29;
            p = (p << 1) | (uint)(ByteParity[t & 0xff] ^ ByteParity[(t >> 8) & 0xff] ^ ByteParity[(t >> 16) & 0xff] ^ ByteParity[(t >> 24)]);

            t = w & PARITY_30;
            p = (p << 1) | (uint)(ByteParity[t & 0xff] ^ ByteParity[(t >> 8) & 0xff] ^ ByteParity[(t >> 16) & 0xff] ^ ByteParity[(t >> 24)]);

            return ((word & 0x3f) == p);
        }

        public static uint GetBitU(byte[] buff, uint pos, uint len)
        {
            uint bits = 0;
            uint i;
            for (i = pos; i < pos + len; i++)
                bits = (uint)((bits << 1) + ((buff[i / 8] >> (int)(7 - i % 8)) & 1u));
            return bits;
        }

        public static void SetBitU(byte[] buff, uint pos, uint len, double data)
        {
            SetBitU(buff, pos, len, (uint)data);
        }


        public static void SetBitU(byte[] buff, uint pos, uint len, uint data)
        {
            var mask = 1u << (int)(len - 1);

            if (len <= 0 || 32 < len) return;

            for (var i = pos; i < pos + len; i++, mask >>= 1)
            {
                if ((data & mask) > 0)
                    buff[i / 8] |= (byte)(1u << (int)(7 - i % 8));
                else
                    buff[i / 8] &= (byte)(~(1u << (int)(7 - i % 8)));
            }
        }

        public override void Reset()
        {
         _state = State.Word1;   
        }
    }


    public class RtcmV2Parser : GnssParserWithMessagesBase<RtcmV2MessageBase,ushort>
    {
        private const byte SyncByte = 0x66;

        public const string GnssProtocolId = "RTCMv2";
        private readonly byte[] _buffer = new byte[33 * 3]; /* message buffer   */
        private uint _word;                /* word buffer for rtcm 2            */
        private int _readedBytes;          /* number of bytes in message buffer */
        private int _readedBits;           /* number of bits in word buffer     */
        private int _len;                  /* message length (bytes)            */

        public RtcmV2Parser(IDiagnostic diag) : base(diag[GnssProtocolId])
        {

        }

        public RtcmV2Parser(IDiagnosticSource diagSource) : base(diagSource)
        {
        }

        private static bool DecodeWord(uint word, byte[] data, int offset)
        {
            var hamming = new uint[] { 0xBB1F3480, 0x5D8F9A40, 0xAEC7CD00, 0x5763E680, 0x6BB1F340, 0x8B7A89C0 };
            uint parity = 0;
            
            if ((word & 0x40000000) != 0) word ^= 0x3FFFFFC0;

            for (var i = 0; i < 6; i++)
            {
                parity <<= 1;
                for (var w = (word & hamming[i]) >> 6; w != 0; w >>= 1) 
                    parity ^= w & 0x1;
            }
            if (parity != (word & 0x3F)) return false;

            for (var i = 0; i < 3; i++) data[i + offset] = (byte)(word >> (22 - i * 8));
            return true;
        }


        public override string ProtocolId => GnssProtocolId;

        public override bool Read(byte data)
        {

            if ((data & 0xC0) != 0x40)
            {
                return false; /* ignore if upper 2bit != 01 */
            }

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

                if (++_readedBits < 30)
                {
                    continue;
                }

                _readedBits = 0;

                /* check parity */
                if (!DecodeWord(_word, _buffer, _readedBytes))
                {
                    Diag.Int["crc err"]++;
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
                var msgType = (ushort)RtcmV3Helper.GetBitU(_buffer, 8, 6);
                ParsePacket(msgType,_buffer);
                Reset();
                return true;
            }
            return false;

        }

        public override void Reset()
        {
            //_word = 0;
            _readedBytes = 0;
            _readedBits = 0;
            _len = 0;
    }

        
    }
}