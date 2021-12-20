using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core
{
    public static class RemoteHandlerMixins
    {
        public static async Task SendPan(this TableHandler handler, int? horizontalOffset, int? verticalOffset)
        {
            await handler.SendAsync(new PanMessage
            {
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset
            });
        }

        public static async Task SendZoom(this TableHandler handler, float zoomAdjustment)
        {
            await handler.SendAsync(new ZoomMessage
            {
                ZoomAdjustment = zoomAdjustment
            });
        }
    }
}
