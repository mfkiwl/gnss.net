using System;
using System.Text;

namespace Asv.Gnss.Control
{
    public abstract class ComNavAsciiCommandBase : ISerializable
    {
        public int GetMaxByteSize()
        {
            return 1024;
        }

        protected abstract string SerializeToAsciiString();
        

        public uint Serialize(byte[] buffer, uint offsetBits)
        {
            var str = SerializeToAsciiString();
            var bytes = Encoding.ASCII.GetBytes(str);
            bytes.CopyTo(buffer, offsetBits / 8);
            var lastByteIndex = offsetBits / 8 + bytes.Length;
            buffer[lastByteIndex] = 0x0D;
            buffer[lastByteIndex + 1] = 0x0A;
            return (uint)(bytes.Length * 8 + offsetBits * 8 + 2 * 8);
        }

        public virtual uint Deserialize(byte[] buffer, uint offsetBits)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return SerializeToAsciiString();
        }
    }
}