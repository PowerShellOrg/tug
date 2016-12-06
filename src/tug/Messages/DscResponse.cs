using tug.Controllers;

namespace tug.Messages
{
    public class DscResponse
    {
        public const string PROTOCOL_VERSION_HEADER = "ProtocolVersion";
        public const string PROTOCOL_VERSION_VALUE = "2.0";


        [ToHeader(Name = PROTOCOL_VERSION_HEADER)]
        public string ProtocolVersionHeader
        { get; set; } = PROTOCOL_VERSION_VALUE;
    }
}