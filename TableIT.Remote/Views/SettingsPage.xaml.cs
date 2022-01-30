using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views;

public partial class SettingsPage
{
    private SettingsPageViewModel ViewModel { get; }
    
    public SettingsPage(SettingsPageViewModel viewModel)
    {
        BindingContext = ViewModel = viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.OnRefresh();
    }
}
