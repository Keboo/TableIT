using System;

namespace TableIT.Core.Messages;

public class LoadImageMessage
{
    public string ImageId { get; set; }
    public string Version { get; set; }
}
