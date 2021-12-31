using System.IO;
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

        public static async Task SendImage(this TableClient client, Stream imageStream)
        {
            //TODO: Compression
            long numBytes = imageStream.Length;
            const int payloadSize = 5_000;

            byte[] bytes = new byte[payloadSize];
            int readBytes;

            while ((readBytes = await imageStream.ReadAsync(bytes, 0, payloadSize)) > 0)
            {
                await client.SendAsync(new LoadImageMessage
                {
                    
                });
            }
        }
    }
}
