using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using System;
using TableIT.Core;

namespace TableIT.Remote
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private ServerHandler _serverHandler;

        public MainPage()
        {
            InitializeComponent();
            _serverHandler = new ServerHandler("Endpoint=https://tableit.service.signalr.net;AccessKey=ilNpv1VeUS5Rn933eEBbgYsQ185epBKDj39/hFdnUfs=;Version=1.0;", "TestHub");
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            count++;
            CounterLabel.Text = $"Current count: {count}";

            SemanticScreenReader.Announce(CounterLabel.Text);
            await _serverHandler.SendRequest("broadcast", "TestHub");
        }
    }
}
