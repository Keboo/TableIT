using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using TableIT.Shared;
using TableIT.Shared.Resources;
using Windows.ApplicationModel.DataTransfer;

namespace TableIT.Win;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private static Uri RootUrl { get; }
#if DEBUG
        = Debugger.IsAttached ? new("https://localhost:7031/") : new("https://tableit.azurewebsites.net/");
#else
        = new("https://tableit.azurewebsites.net/");
#endif

    private ImageManager? _imageManager;
    private ITableConnection? _tableConnection;

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

                if (_tableConnection is { } client)
                {
                    await SetStatusMessage("Loading images...");

                    HttpClient httpClient = new()
                    {
                        BaseAddress = RootUrl
                    };
                    IImageService imageService = new ImageService(httpClient);
                    var imageManager = _imageManager = new(imageService, new ResourcePersistence());
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
            else
            {
                OnZoomToFit();
            }
        });
    }

    private async Task ConnectToServer()
    {
        await SetStatusMessage("Connecting...");

        try
        {
            Uri hostUri = new(RootUrl, "/TableHub");

            _tableConnection = new TableConnection(hostUri);

            _tableConnection.TableConfigurationUpdated += OnTableConfigurationUpdated;
            _tableConnection.ZoomToFit += OnZoomToFit;
            _tableConnection.Zoom += OnZoom;
            _tableConnection.Rotate += OnRotate;
            _tableConnection.Pan += OnPan;
            /*
            _tableConnection.RegisterTableMessage<PanMessage>(message =>
            {
                
            });


            _tableConnection.RegisterTableMessage<RotateMessage>(message =>
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
            */

            string tableId = (Debugger.IsAttached ? "DEBUG1" : TableId.Generate()).ToUpperInvariant();
            await _tableConnection.ConnectAsync(tableId);
            DispatcherQueue.TryEnqueue(
            () =>
            {
                UserId.Text = $"ID: {tableId}";
                ViewUrl.Text = $"https://tableit.keboo.dev/{tableId}";
                Status.Text = "Connected";
            });
        }
        catch (Exception ex)
        {
            await SetStatusMessage($"Error: {ex.Message}");
        }
    }

    private void OnPan(int? horizontalOffset, int? verticalOffset)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Status.Text = $"got pan message {horizontalOffset}x{verticalOffset}";
            double? hOffset = null;
            if (horizontalOffset != null)
            {
                hOffset = ScrollViewer.HorizontalOffset + horizontalOffset;
            }
            double? vOffset = null;
            if (verticalOffset != null)
            {
                vOffset = ScrollViewer.VerticalOffset + verticalOffset;
            }
            ScrollViewer.ChangeView(hOffset, vOffset, null);
        });
    }

    private void OnRotate(int degrees)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Status.Text = $"Rotate message {degrees}";
            double currentRotation = (Image.RenderTransform as RotateTransform)?.Angle ?? 0;

            Image.RenderTransform = new RotateTransform() { Angle = currentRotation += degrees };
            OnZoomToFit();
        });
    }

    private void OnZoom(float zoomAdjustment)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            ScrollViewer.ChangeView(null, null, ScrollViewer.ZoomFactor + zoomAdjustment);
        });
    }

    private void OnZoomToFit()
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

    private async void OnTableConfigurationUpdated(TableConfiguration tableConfiguration)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Status.Text = $"Setting table config";
        });

        if (tableConfiguration.Compass is { } compassConfig)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                Compass.Visibility = compassConfig.IsShown ? Visibility.Visible : Visibility.Collapsed;
                Compass.Width = Compass.Height = compassConfig.Size;
                Compass.Foreground = compassConfig.Color.ToBrush();
            });
        }

        DispatcherQueue.TryEnqueue(() =>
        {
            Status.Text = $"Loading image...";
        });

        if (_imageManager is { } imageManager &&
            tableConfiguration.CurrentResourceId is { })
        {
            Stream imageStream = await imageManager.GetImage(tableConfiguration.CurrentResourceId, null);
            await LoadImage(imageManager, imageStream);
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
        if (_tableConnection?.TableId is { } tableId)
        {
            DataPackage dataPackage = new();
            dataPackage.SetText(tableId);
            Clipboard.SetContent(dataPackage);
        }

    }
}
