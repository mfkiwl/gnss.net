using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Asv.Gnss
{
    public class Nmea0183Parser:GnssParserWithMessagesBase<Nmea0183MessageBase,string>
    {
        public const string GnssProtocolId = "NMEA0183";

        private State _state;
        private readonly byte[] _buffer = new byte[1024];
        private readonly byte[] crcBuffer = new byte[2];
        private int _msgReaded;

        public Nmea0183Parser(IDiagnostic diag):base(diag)
        {

        }

        public override string ProtocolId => GnssProtocolId;

        private enum State
        {
            Sync,
            Msg,
            Crc1,
            Crc2,
            End1,
            End2,
        }



        public override bool Read(byte data)
        {
            switch (_state)
            {
                case State.Sync:
                    if (data == 0x24 /*'$'*/ || data == 0x21/*'!'*/)
                    {
                        _msgReaded = 0;
                        _state = State.Msg;
                    }
                    break;
                case State.Msg:
                    if (data == '*')
                    {
                        _state = State.Crc1;
                    }
                    else
                    {
                        if (_msgReaded >= (_buffer.Length - 2))
                        {
                            // oversize
                            _state = State.Sync;
                        }
                        else
                        {
                            _buffer[_msgReaded] = data;
                            ++_msgReaded;
                        }
                    }
                    break;
                case State.Crc1:
                    crcBuffer[0] = data;
                    _state = State.Crc2;
                    break;
                case State.Crc2:
                    crcBuffer[1] = data;
                    _state = State.End1;
                    break;
                case State.End1:
                    if (data != 0x0D)
                    {
                        Reset();
                        return false;
                    }
                    _state = State.End2;
                    break;
                case State.End2:
                    if (data != 0x0A)
                    {
                        Reset();
                        return false;
                    }
                    var strMessage = Encoding.ASCII.GetString(_buffer, 0, _msgReaded);
                    var readCrc = Encoding.ASCII.GetString(crcBuffer);
                    var calcCrc = CalcCrc(strMessage);
                    if (readCrc == calcCrc)
                    {
                        var msgId = strMessage.Substring(2, 3).ToUpper();
                        var asciiBytes = Encoding.ASCII.GetBytes(strMessage);
                        ParsePacket(msgId,asciiBytes);
                        return true;
                    }
                    else
                    {
                        Diag.Int["crc err"]++;
                        InternalOnError(new GnssParserException(ProtocolId, $"NMEA crc error:'{strMessage}'"));
                    }
                    Reset();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return false;
        }

        private string CalcCrc(string strMessage)
        {
            var crc = 0;
            foreach (var c in strMessage.ToCharArray())
            {
                if (crc == 0)
                {
                    crc = Convert.ToByte(c);
                }
                else
                {
                    crc ^= Convert.ToByte(c);
                }
            }

            return crc.ToString("X2");
        }


        public override void Reset()
        {
            _state = State.Sync;
        }

        
    }


    
}