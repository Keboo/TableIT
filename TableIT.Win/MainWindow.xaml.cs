using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;
using Windows.ApplicationModel.DataTransfer;

namespace TableIT.Win;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private ImageManager? _imageManager;
    private TableClient? _client;

    public MainWindow()
    {
        Title = "Table IT";
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
                await SetStatusMessage($"ERROR: {e.Message}");
            }
        });
    }

    private async Task SetStatusMessage(string message)
    {
        await Task.Yield();
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
            Image.Margin = new Thickness(500);
            Message.Visibility = Visibility.Collapsed;
            if (resourceData is not null)
            {
                ScrollViewer.ChangeView(resourceData.HorizontalOffset, resourceData?.VerticalOffset, resourceData?.ZoomFactor);
            }
        });
    }

    private void ZoomToFit()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (Image.ActualWidth > 0 && Image.ActualHeight > 0)
            {
                
                bool verticalOrientation = false;
                if (Image.RenderTransform is RotateTransform rotate)
                {
                    var angle = rotate.Angle % 360;
                    if (angle > 45 && angle < 135)
                    {
                        verticalOrientation = true;
                    }
                    else if (angle > 135 + 90 && angle < 135 + 180)
                    {
                        verticalOrientation = true;
                    }
                }
                double verticalZoom;
                double horizontalZoom;
                if (verticalOrientation)
                {
                    verticalZoom = ScrollViewer.ViewportHeight / Image.ActualWidth;
                    horizontalZoom = ScrollViewer.ViewportWidth / Image.ActualHeight;
                }
                else
                {
                    verticalZoom = ScrollViewer.ViewportHeight / Image.ActualHeight;
                    horizontalZoom = ScrollViewer.ViewportWidth / Image.ActualWidth;
                }
                var zoom = (float)Math.Min(verticalZoom, horizontalZoom);
                ScrollViewer.ChangeView(Image.Margin.Left * zoom, Image.Margin.Top * zoom, zoom);
            }
        });
    }

    private async Task ConnectToServer()
    {
        await SetStatusMessage("Connecting...");

        try
        {
            if (Debugger.IsAttached)
            {
                _client = new TableClient(userId: "DEBUG1");
            }
            else 
            {
                _client = new TableClient();
            }
            _client.RegisterTableMessage<PanMessage>(message =>
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

            _client.RegisterTableMessage<ZoomMessage>(message =>
            {
                DispatcherQueue.TryEnqueue(
                () =>
                {
                    Status.Text = $"got zoom message {message.ZoomAdjustment} fit? {message.ZoomToFit}";
                    if (message.ZoomToFit is { } fit && fit)
                    {
                        ZoomToFit();
                    }
                    else if (message.ZoomAdjustment is { } adjustment)
                    {
                        ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + adjustment);
                    }
                });
            });

            _client.RegisterTableMessage<LoadImageMessage>(async message =>
            {
                DispatcherQueue.TryEnqueue(
                () =>
                {
                    Status.Text = $"Loading image...";
                });

                if (_imageManager is { } imageManager &&
                    message is { ImageId: not null } &&
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

            _client.RegisterTableMessage<SetTableConfigurationMessage>(message =>
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

            _client.Handle<TablePingRequest, TablePingResponse>(message =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Status.Text = $"New client connected";
                });
                return Task.FromResult<TablePingResponse?>(new TablePingResponse());
            });

            _client.RegisterTableMessage<RotateMessage>(message =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    Status.Text = $"Rotate message {message.RotationDegrees}";
                });

                DispatcherQueue.TryEnqueue(() =>
                {
                    double currentRotation = (Image.RenderTransform as RotateTransform)?.Angle ?? 0;

                    if (message.RotationDegrees is { } rotationDegrees)
                    {
                        Image.RenderTransform = new RotateTransform() { Angle = currentRotation += rotationDegrees };
                    }
                    else
                    {
                        Image.RenderTransform = null;
                    }
                });
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
            Root.Margin = new Thickness(0, 0, 0, 0);
            AppWindow.SetPresenter(AppWindowPresenterKind.Default);
        }
        else
        {
            Root.Margin = new Thickness(0, 5, 0, 0);
            AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
        }
    }

    private void CopyTableId(object sender, RoutedEventArgs e)
    {
        if (_client?.UserId is { } tableId)
        {
            DataPackage dataPackage = new();
            dataPackage.SetText(tableId);
            Clipboard.SetContent(dataPackage);
        }
    }
}
