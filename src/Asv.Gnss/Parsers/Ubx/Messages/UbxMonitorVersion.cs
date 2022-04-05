using System.Collections.Generic;
using System.Text;

namespace Asv.Gnss
{
    public class UbxMonitorVersion : UbxMessageBase
    {
        public override byte Class => 0x0A;
        public override byte SubClass => 0x04;

        /// <summary>
        /// Nul-terminated software version string.
        /// </summary>
        public string Software { get; set; }
        /// <summary>
        /// Nul-terminated hardware version string
        /// </summary>
        public string Hardware { get; set; }
        /// <summary>
        /// Extended software information strings. A series of nul-terminated strings.Each
        /// extension field is 30 characters long and contains varying software information.
        /// Not all extension fields may appear. Examples of reported information: the
        /// software version string of the underlying ROM (when the receiver's firmware is
        /// running from flash), the firmware version, the supported protocol version, the
        /// module identifier, the flash information structure(FIS) file information, the
        /// supported major GNSS, the supported augmentation systems. See Firmware and protocol versions for
        /// details.
        /// </summary>
        public List<string> Extensions { get; set; }

        public override uint Deserialize(byte[] buffer, uint offset)
        {
            GenerateRequest();

            var bitIndex = offset + base.Deserialize(buffer, offset);

            // 40 + 30*N = PayloadLength
            var extLength = (PayloadLength - 40) / 30;

            var stringSize = 0;
            for (var i = 0; i < 30; i++)
            {
                stringSize = i;
                if (buffer[bitIndex + i] == 0)
                {
                    break;
                }
            }

            Software = Encoding.ASCII.GetString(buffer, (int)bitIndex, stringSize);
            bitIndex += 30;

            stringSize = 0;
            for (var i = 0; i < 10; i++)
            {
                stringSize = i;
                if (buffer[bitIndex + i] == 0)
                {
                    break;
                }
            }

            Hardware = Encoding.ASCII.GetString(buffer, (int)bitIndex, stringSize);
            bitIndex += 10;

            Extensions = new List<string>();
            for (var i = 0; i < extLength; i++)
            {
                for (var j = 0; j < 30; j++)
                {
                    stringSize = j;
                    if (buffer[bitIndex + j] == 0)
                    {
                        break;
                    }
                }
                Extensions.Add(Encoding.ASCII.GetString(buffer, (int)bitIndex, stringSize));
                bitIndex += 30;
            }

            return bitIndex - offset;
        }
    }
}