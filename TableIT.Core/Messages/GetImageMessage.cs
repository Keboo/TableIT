using System;

namespace TableIT.Core.Messages
{
    public class MultiPartMessage
    {
        public int TotalParts { get; set; }
        public int Index { get; set; }

        public string? Base64Data { get; set; }
    }

    public class GetImageRequest
    {
        public GetImageRequest()
        { }

        public GetImageRequest(Guid imageId) => ImageId = imageId;

        public Guid ImageId { get; set; }
    }

    public class GetImageResponse : MultiPartMessage
    {
        
    }
}
