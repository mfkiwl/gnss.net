using System;
using System.Text;
using Newtonsoft.Json;

namespace Asv.Gnss
{
    public abstract class Nmea0183MessageBase:GnssMessageBaseWithId<string>
    {
        private string _sourceId;

        public string SourceTitle { get; private set; }

        public string SourceId
        {
            get => _sourceId;
            set
            {
                _sourceId = value;
                SourceTitle = Nmea0183Helper.TryFindSourceTitleById(value);
            }
        }

        public override string ProtocolId => Nmea0183Parser.GnssProtocolId;

        public override int GetMaxByteSize()
        {
            return 1024;
        }

        public override uint Serialize(byte[] buffer, uint offsetBits)
        {
            throw new System.NotImplementedException();
        }

        public override uint Deserialize(byte[] buffer, uint offsetBits)
        {
            if (buffer.Length<5) throw new Exception("Too small string for NMEA");
            var index = (int) (offsetBits / 8);
            var message = Encoding.ASCII.GetString(buffer, index, buffer.Length - index);
            SourceId = message.Substring(0, 2);
            var items = message.Trim().Split(',');
            InternalDeserializeFromStringArray(items);
            return (uint)buffer.Length * 8;
        }

        protected abstract void InternalDeserializeFromStringArray(string[] items);
      
    }
}