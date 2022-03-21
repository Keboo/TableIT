using System;

namespace TableIT.Core.Messages;

public class ResponseMessage
{
    public Guid RequestId { get; set; }
    public string? ResponseType { get; set; }
    public string? ResponseData { get; set; }
}
