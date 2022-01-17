using Microsoft.Maui.Controls;
using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views
{
    public partial class ImageDetailsPage : ContentPage
    {
        public ImageDetailsPageViewModel ViewModel { get; }

        public ImageDetailsPage()
        {

        }

        public ImageDetailsPage(ImageDetailsPageViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            InitializeComponent();
        }
    }
}
