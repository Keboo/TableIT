using System;

namespace TableIT.Core.Messages
{
    public class LoadImageMessage
    {
        public Guid ImageId { get; set; } 
        public string ImageName { get; set; }
        public string Base64Data { get; set; }
    }
}
