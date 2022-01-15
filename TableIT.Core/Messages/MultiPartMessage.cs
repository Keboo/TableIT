using System;

namespace TableIT.Core.Messages
{
    public class EnvelopeMessage
    {
        public Guid GroupId { get; set; }
        public int TotalParts { get; set; }
        public int Index { get; set; }
        public string? Data { get; set; }
        public string? DataType { get; set; }
    }

    public class MultiPartMessage
    {
        public string? Base64Data { get; set; }
    }
}
