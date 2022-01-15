﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core
{
    public static class TableClientMessageSenders
    {
        public static async Task SendPan(this TableClient client, int? horizontalOffset, int? verticalOffset)
        {
            await client.SendAsync(new PanMessage
            {
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset
            });
        }

        public static async Task SendZoom(this TableClient client, float zoomAdjustment)
        {
            await client.SendAsync(new ZoomMessage
            {
                ZoomAdjustment = zoomAdjustment
            });
        }

        public static async Task<IReadOnlyList<ImageData>> GetImages(this TableClient client, CancellationToken? token = null)
        {
            ListImagesResponse? response = await client.SendRequestAsync<ListImagesRequest, ListImagesResponse>(new ListImagesRequest(), token);
            if (response is not null)
            {
                return response.Images;
            }
            return Array.Empty<ImageData>();
        }

        public static async Task<byte[]> GetImage(this TableClient client, Guid imageId, CancellationToken? token = null)
        {
            GetImageResponse? response = await client.SendRequestAsync<GetImageRequest, GetImageResponse>(new GetImageRequest(imageId), token);
            if (response?.Base64Data is not null)
            {
                return Convert.FromBase64String(response.Base64Data);
            }
            return Array.Empty<byte>();
        }

        public static async Task SendImage(this TableClient client, Stream imageStream)
        {
            //TODO: Compression
            long numBytes = imageStream.Length;
            const int payloadSize = 5_000;

            byte[] bytes = new byte[payloadSize];
            int index = 0;
            int totalParts = (int)(numBytes / payloadSize);
            if (numBytes % payloadSize != 0) totalParts++;
            Guid id = Guid.NewGuid();

            while (await imageStream.ReadAsync(bytes, 0, payloadSize) > 0)
            {
                await client.SendAsync(new LoadImageMessage
                {
                    ImageId = id,
                    Base64Data = Convert.ToBase64String(bytes),
                });
            }
        }
    }
}
