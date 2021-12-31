using Microsoft.Maui.Controls;
using TableIT.Remote.Views;

namespace TableIT.Remote
{
    public partial class AppShell : Shell
    {
        public AppShell(ControlPage controlPage, ImagesPage imagesPage)
        {
            InitializeComponent();
            ControlShell!.ContentTemplate = new DataTemplate(() => controlPage);
            ImagesShell!.ContentTemplate = new DataTemplate(() => imagesPage);
        }
    }
}