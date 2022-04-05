using System.Text;

namespace Asv.Gnss
{
    public class UbxInfWarning : UbxMessageBase
    {
        public override byte Class => 0x04;
        public override byte SubClass => 0x01;

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var bitIndex = offsetBits + base.Deserialize(buffer, offsetBits);

            Warning = PayloadLength == 0 ? string.Empty : Encoding.ASCII.GetString(buffer, (int)bitIndex, PayloadLength);

            return bitIndex - offsetBits;
        }

        public string Warning { get; set; }
    }
}