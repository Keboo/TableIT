using System;
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
            await client.SendTableMessage(new PanMessage
            {
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset
            });
        }

        public static async Task SendZoom(this TableClient client, float zoomAdjustment)
        {
            await client.SendTableMessage(new ZoomMessage
            {
                ZoomAdjustment = zoomAdjustment
            });
        }

        public static async Task<bool> PingTable(this TableClient client, CancellationToken token)
        {
            if (await client.SendRequestAsync<TablePingRequest, TablePingResponse>(new TablePingRequest(), token) is not null)
            {
                return true;
            }
            return false;
        }

        private static async Task SendTableMessage<TMessage>(this TableClient client, TMessage message) 
            => await client.SendAsync("tablemessage", message);

        public static void RegisterTableMessage<TMessage>(this TableClient client, Action<TMessage> handler)
            where TMessage : class
            => client.Register<TMessage>(handler);

        private static async Task SendRemoteMessage<TMessage>(this TableClient client, TMessage message) 
            => await client.SendAsync("remotemessage", message);

        public static async Task<IReadOnlyList<ImageData>> GetImages(this TableClient client, CancellationToken? token = null)
        {
            ListImagesResponse? response = await client.SendRequestAsync<ListImagesRequest, ListImagesResponse>(new ListImagesRequest(), token);
            if (response is not null)
            {
                return response.Images;
            }
            return Array.Empty<ImageData>();
        }

        public static async Task<byte[]> GetImage(this TableClient client, 
            Guid imageId, 
            int? width = null, 
            int? height = null, 
            CancellationToken? token = null)
        {
            GetImageResponse? response = await client.SendRequestAsync<GetImageRequest, GetImageResponse>(new GetImageRequest(imageId, width, height), token);
            if (response?.Base64Data is not null)
            {
                return Convert.FromBase64String(response.Base64Data);
            }
            return Array.Empty<byte>();
        }

        public static async Task SendImage(this TableClient client, string name, Stream imageStream)
        {
            Guid id = Guid.NewGuid();
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);

            await client.SendTableMessage(new LoadImageMessage
            {
                ImageId = id,
                ImageName = name,
                Base64Data = Convert.ToBase64String(ms.ToArray()),
            });
        }

        public static async Task SetCurrentImage(this TableClient client, Guid imageId)
        {
            await client.SendTableMessage(new SetImageMessage
            {
                ImageId = imageId,
            });
        }
    }
}
