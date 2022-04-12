using System;
using System.IO.Ports;
using Asv.Tools.Serial;

namespace Asv.Gnss
{

    public class UbxUartConfigurationRequest : UbxPortConfigurationRequest
    {
        protected override byte PortId
        {
            get => 1;
            set => throw new NotImplementedException();
        }
    }

    public class UbxUsbConfigurationRequest : UbxPortConfigurationRequest
    {
        protected override byte PortId
        {
            get => 3;
            set => throw new NotImplementedException();
        }
    }

    public abstract class UbxPortConfigurationRequest : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x00;

        protected abstract byte PortId { get; set; }

        
        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 1;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes;

            buffer[byteIndex++] = PortId;

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            PortId = buffer[byteIndex]; byteIndex++;
            
            return byteIndex * 8 - offsetBits;
        }
    }

    public class UbxPortConfiguration : UbxPortConfigurationRequest
    {
        public bool IsEnable { get; set; }
        public enum PortPolarity
        {
            HighActive = 0,
            LowActive = 1
        }

        public UbxPortConfiguration(PortType type)
        {
            switch (type)
            {
                case PortType.Uart:
                    PortId = 1;
                    SerialPortConfig = new SerialPortConfig();
                    break;
                case PortType.Usb:
                    PortId = 3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override GnssMessageBase GetRequest()
        {
            switch (PortId)
            {
                case 1:
                    return new UbxUartConfigurationRequest();
                case 3:
                    return new UbxUsbConfigurationRequest();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public PortPolarity Polarity { get; set; } = PortPolarity.HighActive;
        public byte Pin { get; set; }
        public ushort Threshold { get; set; }

        public SerialPortConfig SerialPortConfig { get; set; }

        public bool IsInUbxProtocol { get; set; } = true;
        public bool IsInNmeaProtocol { get; set; } = true;
        public bool IsInRtcm2Protocol { get; set; }
        public bool IsInRtcm3Protocol { get; set; } = true;

        public bool IsOutUbxProtocol { get; set; } = true;
        public bool IsOutNmeaProtocol { get; set; } = true;
        public bool IsOutRtcm3Protocol { get; set; } = true;

        public bool IsExtendedTxTimeout { get; set; }

        protected sealed override byte PortId { get; set; }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 19;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offsetBytes)
        {
            var byteIndex = offsetBytes + base.InternalSerialize(buffer, offsetBytes);

            buffer[byteIndex++] = 0;
            var txReady = BitConverter.GetBytes((ushort)((IsEnable ? 1 : 0) | ((byte)Polarity << 1) | (Pin << 2) | (Threshold << 7)));
            buffer[byteIndex++] = txReady[0];
            buffer[byteIndex++] = txReady[1];

            if (SerialPortConfig != null)
            {
                var dataBits = (uint)(SerialPortHelper.GetByteFromCharLength(SerialPortConfig.DataBits) << 6);
                var parity = (uint)(SerialPortHelper.GetByteFromParity(SerialPortConfig.Parity) << 9);
                var stopBits = (uint)(SerialPortHelper.GetByteFromStopBit(SerialPortConfig.StopBits) << 12);
                var uartMode = BitConverter.GetBytes(dataBits | parity | stopBits);
                buffer[byteIndex++] = uartMode[0];
                buffer[byteIndex++] = uartMode[1];
                buffer[byteIndex++] = uartMode[2];
                buffer[byteIndex++] = uartMode[3];

                var boundRate = BitConverter.GetBytes((uint)SerialPortConfig.BoundRate);
                buffer[byteIndex++] = boundRate[0];
                buffer[byteIndex++] = boundRate[1];
                buffer[byteIndex++] = boundRate[2];
                buffer[byteIndex++] = boundRate[3];
            }
            else
            {
                for (var i = 0; i < 8; i++)
                {
                    buffer[byteIndex++] = 0;
                }
            }

            var inProtocol = BitConverter.GetBytes((ushort)((IsInUbxProtocol ? 1 : 0) | ((IsInNmeaProtocol ? 1 : 0) << 1) |
                                                            ((IsInRtcm2Protocol ? 1 : 0) << 2) | ((IsInRtcm3Protocol ? 1 : 0) << 5)));
            buffer[byteIndex++] = inProtocol[0];
            buffer[byteIndex++] = inProtocol[1];

            var outProtocol = BitConverter.GetBytes((ushort)((IsOutUbxProtocol ? 1 : 0) | ((IsOutNmeaProtocol ? 1 : 0) << 1) | ((IsOutRtcm3Protocol ? 1 : 0) << 5)));
            buffer[byteIndex++] = outProtocol[0];
            buffer[byteIndex++] = outProtocol[1];


            var isExtendedTxTimeout = BitConverter.GetBytes((ushort)((IsExtendedTxTimeout ? 1 : 0) << 1));
            buffer[byteIndex++] = isExtendedTxTimeout[0];
            buffer[byteIndex++] = isExtendedTxTimeout[1];

            byteIndex += 2;

            return byteIndex - offsetBytes;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8 + 1;

            var txReady = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            IsEnable = (txReady & 0x01) != 0;
            Polarity = (PortPolarity)((txReady & 0x02) >> 1);
            Pin = (byte)((txReady & 0x7C) >> 2);
            Threshold = (ushort)((txReady & 0xFF80) >> 7);

            if (PortId != 0 && PortId != 3 && PortId != 4) // Serial Port
            {
                var uartMode = BitConverter.ToUInt32(buffer, (int)byteIndex); byteIndex += 4;
                SerialPortConfig = new SerialPortConfig
                {
                    DataBits = SerialPortHelper.GetCharLength((uartMode & 0xC0) >> 6),
                    Parity = SerialPortHelper.GetParity((byte)((uartMode & 0xE00) >> 9)),
                    StopBits = SerialPortHelper.GetStopBit((byte)((uartMode & 0x3000) >> 12)),
                    BoundRate = (int)BitConverter.ToUInt32(buffer, (int)byteIndex)
                };
                byteIndex += 4;
            }
            else
            {
                byteIndex += 8;
            }

            var inProtocol = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            IsInUbxProtocol = (inProtocol & 0x01) != 0;
            IsInNmeaProtocol = (inProtocol & 0x02) != 0;
            IsInRtcm2Protocol = (inProtocol & 0x04) != 0;
            IsInRtcm3Protocol = (inProtocol & 0x20) != 0;

            var outProtocol = BitConverter.ToUInt16(buffer, (int)byteIndex); byteIndex += 2;
            IsOutUbxProtocol = (outProtocol & 0x01) != 0;
            IsOutNmeaProtocol = (outProtocol & 0x02) != 0;
            IsOutRtcm3Protocol = (outProtocol & 0x20) != 0;

            IsExtendedTxTimeout = (BitConverter.ToUInt16(buffer, (int)byteIndex) & 0x02) != 0; byteIndex += 4;

            return byteIndex * 8 - offsetBits;
        }

        private static class SerialPortHelper
        {
            public static int GetCharLength(uint value)
            {
                return value switch
                {
                    0x00 => 5,
                    0x01 => 6,
                    0x02 => 7,
                    0x03 => 8,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }

            public static byte GetByteFromCharLength(int value)
            {
                return value switch
                {
                    5 => 0x00,
                    6 => 0x01,
                    7 => 0x02,
                    8 => 0x03,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }

            public static Parity GetParity(byte value)
            {
                return value switch
                {
                    0 => Parity.Even,
                    1 => Parity.Odd,
                    _ => (value & 0x6) == 4 ? Parity.None : Parity.Space
                };
            }

            public static byte GetByteFromParity(Parity value)
            {
                return value switch
                {
                    Parity.None => 0x04,
                    Parity.Odd => 0x01,
                    Parity.Even => 0x00,
                    Parity.Mark => 0x02,
                    Parity.Space => 0x02,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }

            public static StopBits GetStopBit(byte value)
            {
                return value switch
                {
                    0x00 => StopBits.One,
                    0x01 => StopBits.OnePointFive,
                    0x02 => StopBits.Two,
                    0x03 => StopBits.None,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }

            public static byte GetByteFromStopBit(StopBits value)
            {
                return value switch
                {
                    StopBits.None => 0x03,
                    StopBits.One => 0x00,
                    StopBits.Two => 0x02,
                    StopBits.OnePointFive => 0x01,
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
            }
        }
    }

    public enum PortType
    {
        Uart,
        Usb
    }
}