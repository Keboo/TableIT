using System;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace TableIT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TableClient _client;
        public MainPage()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = "Connecting...";
                });
                try
                {
                    _client = new TableClient();

                    _client.Register<PanMessage>(async message =>
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            Status.Text = $"got pan message {message.HorizontalOffset}x{message.VerticalOffset}";
                            double? horizontalOffset = null;
                            if (message.HorizontalOffset != null)
                            {
                                horizontalOffset = ScrollViewer.HorizontalOffset + message.HorizontalOffset;
                            }
                            double? verticalOffset = null;
                            if (message.VerticalOffset != null)
                            {
                                verticalOffset = ScrollViewer.VerticalOffset + message.VerticalOffset;
                            }
                            ScrollViewer.ChangeView(horizontalOffset, verticalOffset, null);
                        });
                    });

                    _client.Register<ZoomMessage>(async message =>
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () =>
                        {
                            Status.Text = $"got zoom message {message.ZoomAdjustment}";
                            ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + message.ZoomAdjustment);
                        });
                    });

                    await _client.StartAsync();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        UserId.Text = $"ID: {_client.UserId}";
                        Status.Text = "Connected";
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        Status.Text = $"Error: {ex.Message}";
                    });
                }
            });

        }
    }
}
