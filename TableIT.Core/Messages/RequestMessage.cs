using System;

namespace TableIT.Core.Messages
{
    public class RequestMessage
    {
        public Guid RequestId { get; set; }
        public string RequestType { get; set; }
        public string RequestData { get; set; }
    }
}
