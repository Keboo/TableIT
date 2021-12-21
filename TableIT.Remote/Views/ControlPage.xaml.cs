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
            Reset.Clicked += Reset_Clicked;
            
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);
            LayoutButtons();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            InvalidateMeasure();
        }

        private void Reset_Clicked(object sender, System.EventArgs e)
        {
            LayoutButtons();
        }

        private void LayoutButtons()
        {
            AdjustButton(UpButton);
            AdjustButton(RightButton);
            AdjustButton(DownButton);
            AdjustButton(LeftButton);

            static void AdjustButton(Button button)
            {
                button.AnchorX = 0;
                button.AnchorY = 0;

                button.AnchorX = 0.5;
                button.AnchorY = 0.5;
            }
        }
    }
}
