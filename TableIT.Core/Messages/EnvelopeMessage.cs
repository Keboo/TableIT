using System;

namespace TableIT.Core.Messages
{
    public class EnvelopeMessage
    {
        public Guid GroupId { get; set; }
        public string? Data { get; set; }
        public string? DataType { get; set; }
    }
}
