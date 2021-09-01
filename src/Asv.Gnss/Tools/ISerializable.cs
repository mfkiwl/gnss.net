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
        /// <param name="offset"></param>
        /// <returns>writed bytes</returns>
        int Serialize(byte[] buffer, int offset);
        /// <summary>
        /// Deserialize object from buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        int Deserialize(byte[] buffer, int offset);
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

        public int Serialize(byte[] buffer, int offset)
        {
            throw new System.NotImplementedException();
        }

        public int Deserialize(byte[] buffer, int offset)
        {
            throw new System.NotImplementedException();
        }
    }
}