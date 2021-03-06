#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TableIT;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    private ImageManager? _imageManager;
    private TableClient? _client;
    public MainPage()
    {
        InitializeComponent();

        ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;

        Task.Run(async () =>
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

        });

    }

    private async Task SetStatusMessage(string message)
    {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        () =>
        {
            Status.Text = message;
        });
    }

    private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_imageManager is { } imageManager)
        {
            await imageManager.UpdateCurrentImageData(ScrollViewer.HorizontalOffset, ScrollViewer.VerticalOffset, ScrollViewer.ZoomFactor);
        }
    }

    private async Task LoadImage(ImageManager imageManager, Stream imageStream)
    {
        ResourceData? resourceData = await imageManager.GetCurrentData();
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        async () =>
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = $"got zoom message {message.ZoomAdjustment}";
                    ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + message.ZoomAdjustment);
                });
            });

            _client.RegisterTableMessage<LoadImageMessage>(async message =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = $"Setting table config";
                });

                if (message.Config?.Compass is { } compassConfig)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        Compass.Visibility = compassConfig.IsShown ? Visibility.Visible : Visibility.Collapsed;
                        Compass.Width = Compass.Height = compassConfig.Size;
                        Compass.Foreground = compassConfig.Color.ToBrush();
                    });
                }
            });

            _client.Handle<TablePingRequest, TablePingResponse>(async message =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = $"New client connected";
                });
                return new TablePingResponse();
            });

            await _client.StartAsync();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
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
}
