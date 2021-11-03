using System;

namespace Asv.Gnss
{
    [Serializable]
    public class GnssParserException : Exception
    {
        public string ProtocolId { get; }

        public GnssParserException(string protocolId, string message) : base(message)
        {
            ProtocolId = protocolId;
        }

        public GnssParserException(string protocolId, string message, Exception inner) : base(message, inner)
        {
            ProtocolId = protocolId;
        }
    }
}