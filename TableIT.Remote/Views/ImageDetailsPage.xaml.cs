using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using System;
using System.IO;
using TableIT.Remote.ViewModels;

namespace TableIT.Remote.Views
{
    public partial class ImageDetailsPage : ContentPage
    {
        private double currentScale = 1;
        private double startScale = 1;
        private double xOffset = 0;
        private double yOffset = 0;
        private double sX, sY;

        public ImageDetailsPageViewModel ViewModel { get; }

        public ImageDetailsPage(ImageDetailsPageViewModel viewModel)
        {
            BindingContext = ViewModel = viewModel;
            viewModel.PropertyChanged += ViewModel_PropertyChanged;
            InitializeComponent();

            //var pinchGesture = new PinchGestureRecognizer();
            //pinchGesture.PinchUpdated += OnPinchUpdated;
            ////GestureRecognizers.Add(pinchGesture);
            var panGesture = new PanGestureRecognizer();
            panGesture.PanUpdated += OnPanUpdated;
            Canvas.GestureRecognizers.Add(panGesture);
            var tapGesture = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 2
            };
            tapGesture.Tapped += TapGesture_Tapped;
            Canvas.GestureRecognizers.Add(tapGesture);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageDetailsPageViewModel.Image))
            {
                if (ViewModel.Image is { Length: > 0 } imageData)
                {
                    Canvas.Source = ImageSource.FromStream(() =>
                    {
                        return new MemoryStream(ViewModel.Image);
                    });
                }
                else
                {
                    Canvas.Source = null;
                }
                //Canvas.InvalidateSurface();
            }
        }

        private void Canvas_PaintSurface(object? sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();
            if (ViewModel.Image is { Length: > 0 } imageData)
            {
                using SKBitmap bitmap = SKBitmap.Decode(imageData);
                
                canvas.DrawBitmap(bitmap, 0, 0);
            }
        }

        private void TapGesture_Tapped(object? sender, EventArgs e)
        {
            Canvas.TranslationX = 0;
            Canvas.TranslationY = 0;
            //TODO: Should this be scale to fit?
            Canvas.Scale = 1;
        }

        private void OnPinchUpdated(object? sender, PinchGestureUpdatedEventArgs e)
        {
            //if (e.Status == GestureStatus.Started)
            //{
            //    // Store the current scale factor applied to the wrapped user interface element,
            //    // and zero the components for the center point of the translate transform.
            //    startScale = Canvas.Scale;
            //    Canvas.AnchorX = 0;
            //    Canvas.AnchorY = 0;
            //    xOffset = Canvas.TranslationX;
            //    yOffset = Canvas.TranslationY;
            //}
            //else if (e.Status == GestureStatus.Running)
            //{
            //    // Calculate the scale factor to be applied.
            //    currentScale += (e.Scale - 1) * startScale;
            //    currentScale = Math.Max(1, currentScale);

            //    // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
            //    // so get the X pixel coordinate.
            //    double renderedX = Canvas.X + xOffset;
            //    double deltaX = renderedX / Width;
            //    double deltaWidth = Width / (Canvas.Width * startScale);
            //    double originX = (e.ScaleOrigin.X - deltaX) * deltaWidth;

            //    // The ScaleOrigin is in relative coordinates to the wrapped user interface element,
            //    // so get the Y pixel coordinate.
            //    double renderedY = Canvas.Y + yOffset;
            //    double deltaY = renderedY / Height;
            //    double deltaHeight = Height / (Canvas.Height * startScale);
            //    double originY = (e.ScaleOrigin.Y - deltaY) * deltaHeight;

            //    // Calculate the transformed element pixel coordinates.
            //    double targetX = xOffset - (originX * Canvas.Width) * (currentScale - startScale);
            //    double targetY = yOffset - (originY * Canvas.Height) * (currentScale - startScale);

            //    // Apply translation based on the change in origin.
            //    Canvas.TranslationX = targetX.Clamp(-Canvas.Width * (currentScale - 1), 0);
            //    Canvas.TranslationY = targetY.Clamp(-Canvas.Height * (currentScale - 1), 0);

            //    // Apply scale factor.
            //    Canvas.Scale = currentScale;
            //}
            //else if (e.Status == GestureStatus.Completed)
            //{
            //    // Store the translation delta's of the wrapped user interface element.
            //    xOffset = Canvas.TranslationX;
            //    yOffset = Canvas.TranslationY;
            //}
        }

        private void OnPanUpdated(object? sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    sX = Canvas.TranslationX;
                    sY = Canvas.TranslationY;
                    break;
                case GestureStatus.Running:
                    // Translate and ensure we don't pan beyond the wrapped user interface element bounds.
                    //Canvas.TranslationX = Math.Max(Math.Min(0, x + e.TotalX), -Math.Abs(Canvas.Width - Width));
                    //Canvas.TranslationY = Math.Max(Math.Min(0, y + e.TotalY), -Math.Abs(Canvas.Height - Height));
                    Canvas.BatchBegin();
                    Canvas.TranslationX = sX + e.TotalX;
                    Canvas.TranslationY = sY + e.TotalY;

                    Console.WriteLine($"Move {Canvas.TranslationX}x{Canvas.TranslationY}");

                    Canvas.BatchCommit();
                    break;

                case GestureStatus.Completed:
                    // Store the translation applied during the pan
                    //x = Canvas.TranslationX;
                    //y = Canvas.TranslationY;
                    break;
            }
        }
    }
}
