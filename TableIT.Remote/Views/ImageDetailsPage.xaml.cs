using Microsoft.Maui.Controls;
using SkiaSharp;
using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views
{
    public partial class ImageDetailsPage : ContentPage
    {
        public ImageDetailsPageViewModel ViewModel { get; }

        public ImageDetailsPage(ImageDetailsPageViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();
        }


        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageDetailsPageViewModel.Image))
            {
                Canvas.InvalidateSurface();
            }
        }

        private void Canvas_PaintSurface(object? sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            if (ViewModel.Image is { } imageData)
            {
                using SKBitmap bitmap = SKBitmap.Decode(imageData);
                canvas.DrawBitmap(bitmap, 0, 0);
            }
        }
    }
}
