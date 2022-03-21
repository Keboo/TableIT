using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using System;
using Microsoft.UI;
using Microsoft.UI.Windowing;


namespace TableIT.Win;



public static class AppWindowExtensions
{
    public static AppWindow GetAppWindow(this Microsoft.UI.Xaml.Window window)
    {
        IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        return GetAppWindowFromWindowHandle(windowHandle);
    }

    private static AppWindow GetAppWindowFromWindowHandle(IntPtr windowHandle)
    {
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        return AppWindow.GetFromWindowId(windowId);
    }
}


/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private AppWindow AppWindow { get; }
    private ImageManager? _imageManager;
    private TableClient? _client;

    public MainWindow()
    {
        AppWindow = AppWindowExtensions.GetAppWindow(this);
        InitializeComponent();

        ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;

        Task.Run(async () =>
        {
            try
            {
                await ConnectToServer();

                if (_client is { } client)
                {
                    await SetStatusMessage("Loading images...");

                    var imageManager = _imageManager = new(client, new ResourcePersistence());
                    if (await imageManager.Load() is { } imageStream)
                    {
                        await LoadImage(imageManager, imageStream);
                    }

                    await SetStatusMessage("Done");
                }
            }
            catch (Exception e)
            {

            }
        });
    }

    private async Task SetStatusMessage(string message)
    {
        DispatcherQueue.TryEnqueue(
        () =>
        {
            Status.Text = message;
        });
    }

    private async void ScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_imageManager is { } imageManager)
        {
            await imageManager.UpdateCurrentImageData(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset, ScrollViewer.ZoomFactor);
        }
    }

    private async Task LoadImage(ImageManager imageManager, Stream imageStream)
    {
        ResourceData? resourceData = await imageManager.GetCurrentData();
        DispatcherQueue.TryEnqueue(async () =>
        {
            using var _ = imageStream;
            BitmapImage bitmapImage = new();
            await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());
            Image.Source = bitmapImage;
            ScrollViewer.ChangeView(resourceData?.HorizontalOffset ?? 0, resourceData?.VerticalOffset ?? 0, resourceData?.ZoomFactor ?? 1);
        });
    }

    private async Task ConnectToServer()
    {
        await SetStatusMessage("Connecting...");

        try
        {
            _client = new TableClient();
            _client.RegisterTableMessage<PanMessage>(async message =>
            {
                DispatcherQueue.TryEnqueue(
                () =>
                {
                    Status.Text = $"got pan message {message.HorizontalOffset}x{message.VerticalOffset}";
                    double? horizontalOffset = null;
                    if (message.HorizontalOffset != null)
                    {
                        horizontalOffset = ScrollViewer.HorizontalOffset + message.HorizontalOffset;
                    }
                    double? verticalOffset = null;
                    if (message.VerticalOffset != null)
                    {
                        verticalOffset = ScrollViewer.VerticalOffset + message.VerticalOffset;
                    }
                    ScrollViewer.ChangeView(horizontalOffset, verticalOffset, null);
                });
            });

            _client.RegisterTableMessage<ZoomMessage>(async message =>
            {
                DispatcherQueue.TryEnqueue(
                () =>
                {
                    Status.Text = $"got zoom message {message.ZoomAdjustment}";
                    ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + message.ZoomAdjustment);
                });
            });

            _client.RegisterTableMessage<LoadImageMessage>(async message =>
            {
                DispatcherQueue.TryEnqueue(
                () =>
                {
                    Status.Text = $"Loading image {message.ImageId}";
                });

                if (_imageManager is { } imageManager &&
                    await imageManager.GetImage(message.ImageId, message.Version) is { } imageStream)
                {
                    await LoadImage(imageManager, imageStream);
                }
            });

            _client.Handle<TableConfigurationRequest, TableConfigurationResponse>(async message =>
            {
                DispatcherQueue.TryEnqueue(
                () =>
                {
                    Status.Text = $"Getting table config";
                });
                string? currentResourceId = null;
                if (_imageManager is { } imageManager)
                {
                    currentResourceId = (await imageManager.GetCurrentData())?.Id;
                }
                CompassConfiguration? compass = null;
                DispatcherQueue.TryEnqueue(() =>
                {
                    compass = new CompassConfiguration
                    {
                        IsShown = Compass.Visibility == Visibility.Visible,
                        Size = (int)Compass.Width, //NB: height is expected to always match
                        Color = Compass.Foreground.ToPackedColor()
                    };
                });
                return new TableConfigurationResponse
                {
                    Config = new TableConfiguration
                    {
                        Id = _client.UserId,
                        CurrentResourceId = currentResourceId,
                        Compass = compass
                    }
                };
            });

            _client.RegisterTableMessage<SetTableConfigurationMessage>(async message =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Status.Text = $"Setting table config";
                });

                if (message.Config?.Compass is { } compassConfig)
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        Compass.Visibility = compassConfig.IsShown ? Visibility.Visible : Visibility.Collapsed;
                        Compass.Width = Compass.Height = compassConfig.Size;
                        Compass.Foreground = compassConfig.Color.ToBrush();
                    });
                }
            });

            _client.Handle<TablePingRequest, TablePingResponse>(async message =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Status.Text = $"New client connected";
                });
                return new TablePingResponse();
            });

            await _client.StartAsync();
            DispatcherQueue.TryEnqueue(
            () =>
            {
                UserId.Text = $"ID: {_client.UserId}";
                ViewUrl.Text = $"https://tableit.keboo.dev/{_client.UserId}";
                Status.Text = "Connected";
            });
        }
        catch (Exception ex)
        {
            await SetStatusMessage($"Error: {ex.Message}");
        }
    }

    private void ViewUrl_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        if (AppWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.Default);
        }
        else
        {
            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
    }
}
