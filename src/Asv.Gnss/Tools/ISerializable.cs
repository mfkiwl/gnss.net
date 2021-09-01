namespace Asv.Gnss
{
    public interface ISerializable
    {
        /// <summary>
        /// Maximum size in bytes
        /// </summary>
        int GetMaxByteSize();
        /// <summary>
        /// Serialize object to buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offsetBits"></param>
        /// <returns>writed bits!</returns>
        uint Serialize(byte[] buffer, uint offsetBits);
        /// <summary>
        /// Deserialize object from buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offsetBits"></param>
        /// <returns>readed bits! count</returns>
        uint Deserialize(byte[] buffer, uint offsetBits);
    }

    

    public interface IRtcmV3Packet<out TPayload>: ISerializable
        where TPayload: ISerializable
    {

    }

    public class RtcmV3Packet<TPayload> : IRtcmV3Packet<TPayload> where TPayload : ISerializable
    {
        public int GetMaxByteSize()
        {
            return 1024 * 2;
        }

        public uint Serialize(byte[] buffer, uint offset)
        {
            throw new System.NotImplementedException();
        }

        public uint Deserialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }
    }
}