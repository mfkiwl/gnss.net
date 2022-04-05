using System;
using System.IO.Ports;
using Asv.Tools.Serial;

namespace Asv.Gnss
{
    public class UbxPortConfigurationRequest : UbxMessageBase
    {
        public override byte Class => 0x06;
        public override byte SubClass => 0x00;

        public byte PortId { get; set; }

        public override byte[] GenerateRequest()
        {
            return UbxHelper.GenerateRequest(Class, SubClass, new []{PortId});
        }

        public UbxPortConfigurationRequest()
        {
        }

        public UbxPortConfigurationRequest(PortType portType)
        {
            PortId = portType switch
            {
                PortType.Uart => 1,
                PortType.Usb => 3,
                _ => throw new ArgumentOutOfRangeException(nameof(portType), portType, null)
            };
        }

        public enum PortType
        {
            Uart = 0,
            Usb = 1
        }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 1;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset;

            buffer[bitIndex++] = PortId;

            return bitIndex - offset;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            PortId = buffer[bitIndex]; bitIndex++;
            
            return bitIndex - offsetBits;
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

        public PortPolarity Polarity { get; set; }
        public byte Pin { get; set; }
        public ushort Threshold { get; set; }

        public SerialPortConfig SerialPortConfig { get; set; }

        public bool IsInUbxProtocol { get; set; }
        public bool IsInNmeaProtocol { get; set; }
        public bool IsInRtcm2Protocol { get; set; }
        public bool IsInRtcm3Protocol { get; set; }

        public bool IsOutUbxProtocol { get; set; }
        public bool IsOutNmeaProtocol { get; set; }
        public bool IsOutRtcm3Protocol { get; set; }

        public bool IsExtendedTxTimeout { get; set; }

        public override int GetMaxByteSize()
        {
            return base.GetMaxByteSize() + 19;
        }

        public UbxPortConfiguration()
        {
        }

        protected UbxPortConfiguration(PortType portType) : base(portType)
        {
            if (portType == PortType.Uart)
            {
                SerialPortConfig = new SerialPortConfig
                {
                    BoundRate = 115200
                };
            }

        }

        public static byte[] SetUart(int boundRate = 115200)
        {
            var msg = new UbxPortConfiguration(PortType.Uart)
            {
                IsInUbxProtocol = true,
                IsInNmeaProtocol = true,
                IsInRtcm2Protocol = false,
                IsInRtcm3Protocol = true,
                IsOutUbxProtocol = true,
                IsOutNmeaProtocol = true,
                IsOutRtcm3Protocol = true,
                SerialPortConfig = {BoundRate = boundRate}
            };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        public static byte[] SetUsb()
        {
            var msg = new UbxPortConfiguration(PortType.Usb)
            {
                IsInUbxProtocol = true,
                IsInNmeaProtocol = true,
                IsInRtcm2Protocol = false,
                IsInRtcm3Protocol = true,
                IsOutUbxProtocol = true,
                IsOutNmeaProtocol = true,
                IsOutRtcm3Protocol = true
            };
            var result = new byte[msg.GetMaxByteSize()];
            msg.Serialize(result, 0);
            return result;
        }

        protected override uint InternalSerialize(byte[] buffer, uint offset)
        {
            var bitIndex = offset + base.InternalSerialize(buffer, offset) + 1;

            var txReady = BitConverter.GetBytes((ushort)((IsEnable ? 1 : 0) | ((byte)Polarity << 1) | (Pin << 2) | (Threshold << 7)));
            buffer[bitIndex++] = txReady[0];
            buffer[bitIndex++] = txReady[1];

            if (SerialPortConfig != null)
            {
                var dataBits = (uint)(SerialPortHelper.GetByteFromCharLength(SerialPortConfig.DataBits) << 6);
                var parity = (uint)(SerialPortHelper.GetByteFromParity(SerialPortConfig.Parity) << 9);
                var stopBits = (uint)(SerialPortHelper.GetByteFromStopBit(SerialPortConfig.StopBits) << 12);
                var uartMode = BitConverter.GetBytes(dataBits | parity | stopBits);
                buffer[bitIndex++] = uartMode[0];
                buffer[bitIndex++] = uartMode[1];
                buffer[bitIndex++] = uartMode[2];
                buffer[bitIndex++] = uartMode[3];

                var boundRate = BitConverter.GetBytes((uint)SerialPortConfig.BoundRate);
                buffer[bitIndex++] = boundRate[0];
                buffer[bitIndex++] = boundRate[1];
                buffer[bitIndex++] = boundRate[2];
                buffer[bitIndex++] = boundRate[3];
            }
            else
            {
                for (var i = bitIndex; i < bitIndex + 8; i++)
                {
                    buffer[i] = 0;
                }
                bitIndex += 8;
            }

            var inProtocol = BitConverter.GetBytes((ushort)((IsInUbxProtocol ? 1 : 0) | ((IsInNmeaProtocol ? 1 : 0) << 1) |
                                                            ((IsInRtcm2Protocol ? 1 : 0) << 2) | ((IsInRtcm3Protocol ? 1 : 0) << 5)));
            buffer[bitIndex++] = inProtocol[0];
            buffer[bitIndex++] = inProtocol[1];

            var outProtocol = BitConverter.GetBytes((ushort)((IsOutUbxProtocol ? 1 : 0) | ((IsOutNmeaProtocol ? 1 : 0) << 1) | ((IsOutRtcm3Protocol ? 1 : 0) << 5)));
            buffer[bitIndex++] = outProtocol[0];
            buffer[bitIndex++] = outProtocol[1];


            var isExtendedTxTimeout = BitConverter.GetBytes((ushort)((IsExtendedTxTimeout ? 1 : 0) << 1));
            buffer[bitIndex++] = isExtendedTxTimeout[0];
            buffer[bitIndex++] = isExtendedTxTimeout[1];

            bitIndex += 2;

            return bitIndex - offset;
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits) + 1;

            var txReady = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            IsEnable = (txReady & 0x01) != 0;
            Polarity = (PortPolarity)((txReady & 0x02) >> 1);
            Pin = (byte)((txReady & 0x7C) >> 2);
            Threshold = (ushort)((txReady & 0xFF80) >> 7);

            if (PortId != 0 && PortId != 3 && PortId != 4) // Serial Port
            {
                var uartMode = BitConverter.ToUInt32(buffer, (int)bitIndex); bitIndex += 4;
                SerialPortConfig = new SerialPortConfig
                {
                    DataBits = SerialPortHelper.GetCharLength((uartMode & 0xC0) >> 6),
                    Parity = SerialPortHelper.GetParity((byte)((uartMode & 0xE00) >> 9)),
                    StopBits = SerialPortHelper.GetStopBit((byte)((uartMode & 0x3000) >> 12)),
                    BoundRate = (int)BitConverter.ToUInt32(buffer, (int)bitIndex)
                };
                bitIndex += 4;
            }
            else
            {
                bitIndex += 8;
            }

            var inProtocol = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            IsInUbxProtocol = (inProtocol & 0x01) != 0;
            IsInNmeaProtocol = (inProtocol & 0x02) != 0;
            IsInRtcm2Protocol = (inProtocol & 0x04) != 0;
            IsInRtcm3Protocol = (inProtocol & 0x20) != 0;

            var outProtocol = BitConverter.ToUInt16(buffer, (int)bitIndex); bitIndex += 2;
            IsOutUbxProtocol = (outProtocol & 0x01) != 0;
            IsOutNmeaProtocol = (outProtocol & 0x02) != 0;
            IsOutRtcm3Protocol = (outProtocol & 0x20) != 0;

            IsExtendedTxTimeout = (BitConverter.ToUInt16(buffer, (int)bitIndex) & 0x02) != 0; bitIndex += 4;

            return bitIndex - offsetBits;
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
}