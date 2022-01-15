using System;

namespace TableIT.Core.Messages
{
    public class GetImageRequest
    {
        public GetImageRequest()
        { }

        public GetImageRequest(Guid imageId) => ImageId = imageId;

        public Guid ImageId { get; set; }
    }

    public class GetImageResponse
    {
        public string? Base64Data { get; set; }
    }
}
