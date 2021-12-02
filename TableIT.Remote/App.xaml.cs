using Application = Microsoft.Maui.Controls.Application;

namespace TableIT.Remote
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
