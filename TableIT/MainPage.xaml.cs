using System;
using System.Threading.Tasks;
using TableIT.Core;
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
        ClientHandler _client;
        ServerHandler _server;
        //HubConnection hb;
        public MainPage()
        {
            InitializeComponent();

            //ServiceEndpoint se = new("Endpoint=https://tableit.service.signalr.net;AccessKey=ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=;Version=1.0;");

            Task.Run(async () =>
            {
                try
                {
                    _client = new ClientHandler("Endpoint=https://tableit.service.signalr.net;AccessKey=ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=;Version=1.0;", 
                        "TestHub", "test-user", 
                        async data => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Text.Text = data));

                    await _client.StartAsync();
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
