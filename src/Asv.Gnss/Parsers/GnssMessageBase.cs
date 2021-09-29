using Newtonsoft.Json;

namespace Asv.Gnss
{
    public abstract class GnssMessageBase : ISerializable
    {
        /// <summary>
        /// This is for custom use (like routing, etc...)
        /// </summary>
        public object Tag { get; set; }

        public abstract string ProtocolId { get; }
        public abstract int GetMaxByteSize();
        public abstract uint Serialize(byte[] buffer, uint offsetBits);
        public abstract uint Deserialize(byte[] buffer, uint offsetBits);

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}