using Microsoft.Maui.Controls;
using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views
{
    public partial class DisconnectPage : ContentPage
    {
        private DisconnectPageViewModel ViewModel { get; }
        public DisconnectPage(DisconnectPageViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.Disconnect();
        }
    }
}
