using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Remote.Imaging;

namespace TableIT.Remote.ViewModels
{
    public class ImageDetailsPageViewModel : ObservableObject, IQueryAttributable
    {
        public ImageDetailsPageViewModel(
            TableClientManager clientManager,
            IImageManager imageManager)
        {
            ClientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
            ImageManager = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
            SelectCommand = new AsyncRelayCommand(OnSelect);
        }

        private async Task OnSelect()
        {
            if (RemoteImage is { } remoteImage &&
                ClientManager.GetClient() is { } client)
            {
                await client.SetCurrentImage(remoteImage.ImageId);
            }
        }

        public IAsyncRelayCommand SelectCommand { get; }
        private TableClientManager ClientManager { get; }
        private IImageManager ImageManager { get; }


        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private RemoteImage? _remoteImage;
        private RemoteImage? RemoteImage
        {
            get => _remoteImage;
            set
            {
                if (_remoteImage != value)
                {
                    _remoteImage = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }
        public byte[]? Image => RemoteImage?.ImageData;

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            IsLoading = true;
            if (query.TryGetQueryParamter("imageId", out string? imageId) &&
                await ImageManager.FindImage(imageId) is { } remoteImage)
            {
                RemoteImage = await ImageManager.LoadImage(remoteImage);
            }
            IsLoading = false;
        }
    }
}
