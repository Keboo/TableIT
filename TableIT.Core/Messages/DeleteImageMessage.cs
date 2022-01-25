using System;

namespace TableIT.Core.Messages;

public class DeleteImageRequest
{
    public string ImageId { get; set; }
}

public class DeleteImageResponse
{
    public bool WasDeleted { get; set; }
}