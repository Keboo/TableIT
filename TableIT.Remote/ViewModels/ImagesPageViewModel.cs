using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;

namespace TableIT.Remote.ViewModels
{
    public class ImageViewModel : ObservableObject
    {
        public ImageViewModel(ImageData data)
        {
            Data = data;
        }

        public ImageData Data { get; }
        public string? Name => Data.Name;

        private ImageSource? _image;
        public ImageSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
    }

    public class ImagesPageViewModel : ObservableObject
    {
        public IRelayCommand PickImage { get; }
        public TableClientManager ClientManager { get; }

        private IReadOnlyList<ImageViewModel>? _images;
        public IReadOnlyList<ImageViewModel>? Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public ImagesPageViewModel(TableClientManager clientManager)
        {
            PickImage = new AsyncRelayCommand(OnPickImage);
            ClientManager = clientManager;
        }

        public async Task LoadImages()
        {
            if (ClientManager.GetClient() is { } client)
            {
                Images = (await client.GetImages())
                    .Select(x => new ImageViewModel(x))
                    .ToList();

                await Task.WhenAll(Images.Select(async x => {
                    byte[] data = await client.GetImage(x.Data.Id);
                    var ms = new MemoryStream(data);
                    x.Image = ImageSource.FromStream(() => ms);
                }));
            }
        }

        private async Task OnPickImage()
        {
            if (await PickAndShow(PickOptions.Images) is { } fileResult)
            {
                using Stream stream = await fileResult.OpenReadAsync();

                //await Client.SendImage(stream);
            }
        }

        private async Task<FileResult?> PickAndShow(PickOptions options)
        {
            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    //Text = $"File Name: {result.FileName}";
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        //Stream stream = await result.OpenReadAsync();


                        //Image = ImageSource.FromStream(() => stream);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
            }

            return null;
        }
    }
}
