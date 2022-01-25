using System;
using System.Collections.Generic;
using System.IO;
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

        public static async Task<bool> PingTable(this TableClient client)
        {
            if (await client.SendRequestAsync<TablePingRequest, TablePingResponse>(new TablePingRequest()) is not null)
            {
                return true;
            }
            return false;
        }

        public static async Task<bool> DeleteImage(this TableClient client, string imageId)
        {
            DeleteImageRequest request = new() { ImageId = imageId };
            if (await client.SendRequestAsync<DeleteImageRequest, DeleteImageResponse>(request) is { } response)
            {
                return response.WasDeleted;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static async Task SendTableMessage<TMessage>(this TableClient client, TMessage message)
            => await client.SendAsync("tablemessage", message);

        public static void RegisterTableMessage<TMessage>(this TableClient client, Action<TMessage> handler)
            where TMessage : class
            => client.Register<TMessage>(handler);

        private static async Task SendRemoteMessage<TMessage>(this TableClient client, TMessage message) 
            => await client.SendAsync("remotemessage", message);

        public static async Task SendImage(
            this TableClient client,
            string name,
            Stream imageStream,
            string id,
            IProgress<double>? progress = null)
        {
            using var ms = new MemoryStream();
            await imageStream.CopyToAsync(ms);

            //await client.SendTableMessage(new LoadImageMessage
            //{
            //    ImageId = id,
            //    ImageName = name,
            //    Base64Data = Convert.ToBase64String(ms.ToArray()),
            //});
        }

        public static async Task SetCurrentImage(this TableClient client, string imageId)
        {
            await client.SendTableMessage(new SetImageMessage
            {
                ImageId = imageId,
            });
        }
    }
}
