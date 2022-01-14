using System;

namespace TableIT.Core.Messages
{
    public class LoadImageMessage : MultiPartMessage
    {
        public Guid ImageId { get; set; } 
    }
}
