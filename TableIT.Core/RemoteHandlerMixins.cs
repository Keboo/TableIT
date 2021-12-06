using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core
{
    public static class RemoteHandlerMixins
    {
        public static async Task SendPan(this RemoteHandler handler, int? horizontalOffset, int? verticalOffset)
        {
            await handler.SendRequest(new PanMessage
            {
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset
            });
        }

        public static async Task SendZoom(this RemoteHandler handler, float zoomAdjustment)
        {
            await handler.SendRequest(new ZoomMessage
            {
                ZoomAdjustment = zoomAdjustment
            });
        }
    }
}
