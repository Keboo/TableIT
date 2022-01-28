#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TableIT
{
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

            Task.Run(async () =>
            {
                await ConnectToServer();
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = "Loading images...";
                });
                if (_client is { } client)
                {
                    _imageManager = new(client);
                    await _imageManager.Load();
                    if (await _imageManager.GetCurrentImage() is { } image)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            Image.Source = await image.GetImageSource();
                        });
                    }
                }

            });

        }

        private async Task ConnectToServer()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = "Connecting...";
                });
            try
            {
#if DEBUG
                _client = new TableClient(new ResourcePersistence(), userId: "DEBUG2");
#else
                _client = new TableClient();
#endif
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
                        await imageManager.GetImage(message.ImageId) is { } imageStream)
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        async () =>
                        {
                            BitmapImage bitmapImage = new();
                            await bitmapImage.SetSourceAsync(imageStream.AsRandomAccessStream());
                            Image.Source = bitmapImage;
                        });
                    }
                });

                //_client.RegisterTableMessage<SetImageMessage>(async message =>
                //{
                //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                //    () =>
                //    {
                //        Status.Text = $"Setting image {message.ImageId}";
                //    });
                //    if (await _imageManager.SetCurrentImage(message.ImageId) is { } image)
                //    {
                //        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                //        async () =>
                //        {
                //            Image.Source = await image.GetImageSource();
                //        });
                //    }
                //});

                //_client.Handle<ListImagesRequest, ListImagesResponse>(async message =>
                //{
                //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                //    () =>
                //    {
                //        Status.Text = $"Listing images";
                //    });
                //    var response = new ListImagesResponse();
                //    await foreach (Image image in _imageManager.GetImages())
                //    {
                //        response.Images.Add(new ImageData
                //        {
                //            Id = image.Id,
                //            Name = image.Name 
                //        });
                //    }
                //    return response;
                //});

                //_client.Handle<GetImageRequest, GetImageResponse>(async message =>
                //{
                //    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                //    () =>
                //    {
                //        Status.Text = $"Getting image {message.ImageId}";
                //    });
                //    var response = new GetImageResponse();
                //    await foreach (Image image in _imageManager.GetImages())
                //    {
                //        if (image.Id == message.ImageId)
                //        {
                //            response.Base64Data = Convert.ToBase64String(await image.GetBytes(message.Width, message.Height));
                //        }
                //    }
                //    return response;
                //});

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
                    Status.Text = "Connected";
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    Status.Text = $"Error: {ex.Message}";
                });
            }
        }
    }
}
