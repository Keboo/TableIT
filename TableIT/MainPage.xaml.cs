using Windows.UI.Xaml.Controls;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Client;
using System;
using Windows.UI.Core;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TableIT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        HubConnection hb;
        public MainPage()
        {
            InitializeComponent();

            var hb = new HubConnection("https://tableitserver.azurewebsites.net", true);
            var hubProxy = hb.CreateHubProxy("BroadcastHub");

            hb.Received += Hb_Received;
            hb.StateChanged += Hb_StateChanged;
            hb.Reconnecting += Hb_Reconnecting;
            hb.Reconnected += Hb_Reconnected;
            hubProxy.On<DateTime>("Broadcast",
                          async data =>
                                await Dispatcher
                                      .RunAsync(CoreDispatcherPriority.Normal,
                                                () => Text.Text = data.ToString()));
            hb.Start();

        }

        private void Hb_Reconnected()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Text.Text = $"Reconnected";
            });
        }

        private void Hb_Reconnecting()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Text.Text = $"Reconnecting...";
            });
        }

        private void Hb_StateChanged(StateChange obj)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Text.Text = $"State: {obj.OldState} => {obj.NewState}";
            });
        }

        private void Hb_Received(string obj)
        {
            Text.Text = obj;
        }
    }
}
