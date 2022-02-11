using Microsoft.Maui.Dispatching;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using TableIT.Core;
using TableIT.Core.Messages;

namespace TableIT.Remote.ViewModels;

public class SettingsPageViewModel : ObservableObject
{
    private const uint DarkColor = 0xFF000000;
    private const uint LightColor = 0xFFFFFFFF;

    private TableClientManager ClientManager { get; }

    public ICommand RefreshCommand { get; }
    public IRelayCommand SaveCommand { get; }
    
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string? _tableId;
    public string? TableId
    {
        get => _tableId;
        private set => SetProperty(ref _tableId, value);
    }

    private bool _isCompassShown;
    public bool IsCompassShown
    {
        get => _isCompassShown;
        set => SetProperty(ref _isCompassShown, value);
    }

    private bool _isDarkColor;
    public bool IsDarkColor
    {
        get => _isDarkColor;
        set => SetProperty(ref _isDarkColor, value);
    }

    private int _compassSize;
    public int CompassSize
    {
        get => _compassSize;
        set => SetProperty(ref _compassSize, value);
    }

    public IDispatcher Dispatcher { get; }

    public SettingsPageViewModel(TableClientManager clientManager, IDispatcher dispatcher)
    {
        ClientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        RefreshCommand = new AsyncRelayCommand(OnRefresh);
        SaveCommand = new AsyncRelayCommand(OnSave, () => !IsLoading);
    }

    internal async Task OnRefresh()
    {
        try
        {
            IsLoading = true;
            if (ClientManager.GetClient() is { } client)
            {
                if (await client.GetTableConfiguration() is { } config)
                {
                    _tableId = config.Id;
                    OnPropertyChanged(nameof(TableId));

                    _isCompassShown = config.Compass?.IsShown == true;
                    OnPropertyChanged(nameof(IsCompassShown));

                    _isDarkColor = config.Compass?.Color == DarkColor;
                    OnPropertyChanged(nameof(IsDarkColor));

                    _compassSize = config.Compass?.Size ?? 0;
                    OnPropertyChanged(nameof(CompassSize));
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task OnSave()
    {
        if (IsLoading) return;
        try
        {
            IsLoading = true;
            if (ClientManager.GetClient() is { } client)
            {
                TableConfiguration config = new()
                {
                    Compass = new()
                    {
                        IsShown = IsCompassShown,
                        Size = CompassSize,
                        Color = IsDarkColor ? DarkColor : LightColor
                    }
                };
                await client.UpdateTableConfiguration(config);
            }
        }
        finally
        {
            IsLoading = false;
        }
        await OnRefresh();
    }
}
