namespace Asv.Gnss
{
    public interface ISerializable
    {
        void Serialize(byte[] buffer, uint startIndex = 0);
        void Deserialize(byte[] buffer, uint startIndex = 0);
    }
}