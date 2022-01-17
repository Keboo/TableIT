using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Remote.Imaging;
using TableIT.Remote.Messages;

namespace TableIT.Remote.ViewModels
{
    public class ImagesPageViewModel : ObservableObject
    {
        public IRelayCommand ImportCommand { get; }
        public IRelayCommand RefreshCommand { get; }
        public TableClientManager ClientManager { get; }
        public IImageManager ImageManager { get; }
        public IMessenger Messenger { get; }

        private IReadOnlyList<ImageViewModel>? _images;
        public IReadOnlyList<ImageViewModel>? Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public ImagesPageViewModel(
            TableClientManager clientManager,
            IImageManager imageManager, 
            IMessenger messenger)
        {
            ImportCommand = new AsyncRelayCommand(OnImport);
            RefreshCommand = new AsyncRelayCommand(LoadImages);
            ClientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            ImageManager = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
            Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        }

        public async Task LoadImages()
        {
            Images = (await ImageManager.LoadImages(true))
                .Select(x => new ImageViewModel(x))
                .ToList();

            await Task.WhenAll(Images.Select(async x => 
            {
                if (await ImageManager.LoadThumbnailImage(x.Data) is { } remoteImage)
                {
                    x.Image = remoteImage.Thumbnail;
                }
            }));
        }

        private async Task OnImport()
        {
            if (await PickAndShow(PickOptions.Images) is { } fileResult)
            {
                using Stream stream = await fileResult.OpenReadAsync();
                //TODO: prompt for name
                await ClientManager.GetClient().SendImage(fileResult.FileName, stream);
            }
        }

        public void OnItemSelected(ImageViewModel imageViewModel)
            => Messenger.Send(new ImageSelected(imageViewModel.Data.ImageId));

        private static async Task<FileResult?> PickAndShow(PickOptions options)
        {
            try
            {
                FileResult? result = await Device.InvokeOnMainThreadAsync(async () => await FilePicker.PickAsync(options));
                return result;
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
                return null;
            }
        }
    }
}
