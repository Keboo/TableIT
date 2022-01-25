using Microsoft.Maui.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using TableIT.Remote.Imaging;

namespace TableIT.Remote.ViewModels
{
    public class ImageViewModel : ObservableObject
    {
        public ImageViewModel(RemoteImage data)
        {
            Data = data;
        }

        public RemoteImage Data { get; }
        public string Name => Data.Name;

        private ImageSource? _image;
        public ImageSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        private double _progress = -1;
        public double Progress
        {
            get => _progress;
            set
            {
                SetProperty(ref _progress, value);
                Debug.WriteLine($"Progress {value}");
            }
        }
    }
}
