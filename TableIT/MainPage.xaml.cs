using System;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TableIT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private TableHandler _client;
        public MainPage()
        {
            InitializeComponent();

            //ServiceEndpoint se = new("Endpoint=https://tableit.service.signalr.net;AccessKey=ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=;Version=1.0;");

            Task.Run(async () =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                Text.Text = "Connecting...";
                            });
                try
                {
                    _client = new TableHandler(
                        "https://tableit.service.signalr.net",
                        "ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=",
                        "TestHub",
                        "test-user");
                    _client.Register<PanMessage>(async message =>
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                Text.Text = $"got pan message {message.HorizontalOffset}x{message.VerticalOffset}";
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
                                Text.Text = $"got zoom message {message.ZoomAdjustment}";
                                ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + message.ZoomAdjustment);
                            });
                    });

                    await _client.StartAsync();
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            () =>
                            {
                                Text.Text = "Connected";
                            });
                }
                catch (Exception ex)
                { }
            });

            //Task.Run(async () =>
            //{
            //    try
            //    {
            //        _server = new ServerHandler("Endpoint=https://tableit.service.signalr.net;AccessKey=ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=;Version=1.0;", "TestHub");

            //        await _server.Start();
            //    }
            //    catch(Exception ex)
            //    { }
            //});
            //hb.Received += Hb_Received;
            //hb.StateChanged += Hb_StateChanged;
            //hb.Reconnecting += Hb_Reconnecting;
            //hb.Reconnected += Hb_Reconnected;
            //hubProxy.On<DateTime>("Broadcast",
            //              async data =>
            //                    await Dispatcher
            //                          .RunAsync(CoreDispatcherPriority.Normal,
            //                                    () => Text.Text = data.ToString()));
            //hb.Start();

        }

        //private void Hb_Reconnected()
        //{
        //    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        Text.Text = $"Reconnected";
        //    });
        //}

        //private void Hb_Reconnecting()
        //{
        //    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        Text.Text = $"Reconnecting...";
        //    });
        //}

        //private void Hb_StateChanged(StateChange obj)
        //{
        //    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        Text.Text = $"State: {obj.OldState} => {obj.NewState}";
        //    });
        //}

        private void Hb_Received(string obj)
        {
            Text.Text = obj;
        }
    }
}
