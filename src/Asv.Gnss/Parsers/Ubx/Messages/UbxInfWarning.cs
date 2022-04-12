using System.Text;

namespace Asv.Gnss
{
    public class UbxInfWarning : UbxMessageBase
    {
        public override byte Class => 0x04;
        public override byte SubClass => 0x01;

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            var byteIndex = (offsetBits + base.Deserialize(buffer, offsetBits)) / 8;

            Warning = PayloadLength == 0 ? string.Empty : Encoding.ASCII.GetString(buffer, (int)byteIndex, PayloadLength);

            return byteIndex * 8 - offsetBits;
        }

        public string Warning { get; set; }
    }
}