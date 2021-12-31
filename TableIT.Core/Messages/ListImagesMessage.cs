using System;
using System.Collections.Generic;

namespace TableIT.Core.Messages
{
    public class RequestMessage
    {
        public Guid RequestId { get; set; }
        public string RequestType { get; set; }
        public string RequestData { get; set; }
    }

    public class ResponseMessage
    {
        public Guid RequestId { get; set; }
        public string ResponseType { get; set; }
        public string ResponseData { get; set; }
    }

    public class ListImagesRequest
    {

    }

    public class ListImagesResponse
    {
        public List<string> Images { get; set; } = new();
    }
}
