﻿using System;
using System.Collections.Generic;

namespace TableIT.Core.Messages
{
    public class ListImagesRequest
    { }

    public class ListImagesResponse
    {
        public List<ImageData> Images { get; set; } = new();
    }

    public class ImageData
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }
}