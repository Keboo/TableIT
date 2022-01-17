using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using TableIT.Remote.Imaging;

namespace TableIT.Remote.ViewModels
{
    public class ImageDetailsPageViewModel : ObservableObject, IQueryAttributable
    {

        public ImageDetailsPageViewModel(IImageManager imageManager)
        {
            ImageManager = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
            SelectCommand = new RelayCommand(OnSelect);
        }

        private void OnSelect()
        {
            
        }

        public ICommand SelectCommand { get; }

        private IImageManager ImageManager { get; }

        private RemoteImage? _remoteImage;
        private RemoteImage? RemoteImage
        {
            get => _remoteImage;
            set
            {
                _remoteImage = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        public ImageSource? Image => RemoteImage?.Image;

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetQueryParamter("imageId", out Guid imageId) &&
                await ImageManager.FindImage(imageId) is { } remoteImage)
            {
                RemoteImage = await ImageManager.LoadImage(remoteImage);
            }
        }
    }


}
