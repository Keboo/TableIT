using Microsoft.Maui.Controls;
using TableIT.Remote.ViewModels;
using System.Threading.Tasks;

namespace TableIT.Remote.Views
{
    public partial class ImagesPage : ContentPage
    {
        private ImagesPageViewModel ViewModel { get; }

        public ImagesPage(ImagesPageViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
            ViewModel.DisplayPrompt += ViewModel_DisplayPrompt;
        }

        private Task<string?> ViewModel_DisplayPrompt()
        {
            return DisplayPromptAsync("Image name", "Enter the name of the image");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await ViewModel.LoadImages();
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is ImageViewModel imageViewModel)
            {
                ViewModel.OnItemSelected(imageViewModel);
            }
        }
    }
}
