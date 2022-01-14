using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
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
}
