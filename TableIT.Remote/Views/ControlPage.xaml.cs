using Microsoft.Maui.Controls;
using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views
{
    public partial class ControlPage : ContentPage
    {
        
        public ControlPage()
        {
            BindingContext = new ControlPageViewModel();
            InitializeComponent();

            AdjustButton(UpButton);
            AdjustButton(RightButton);
            AdjustButton(DownButton);
            AdjustButton(LeftButton);

            static void AdjustButton(Button button)
            {
                button.AnchorX = 0;
                button.AnchorX = 0.5;
                button.AnchorY = 0;
                button.AnchorY = 0.5;
            }
        }
    }
}
