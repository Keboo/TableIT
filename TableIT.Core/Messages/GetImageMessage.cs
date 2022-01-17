using System;

namespace TableIT.Core.Messages
{
    public class GetImageRequest
    {
        public GetImageRequest()
        { }

        public GetImageRequest(Guid imageId, int? width, int? height)
        {
            ImageId = imageId;
            Width = width;
            Height = height;
        }

        public Guid ImageId { get; set; }

        public int? Height { get; set; }
        public int? Width { get; set; }
    }

    public class GetImageResponse
    {
        public string? Base64Data { get; set; }
    }
}
