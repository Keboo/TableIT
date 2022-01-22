﻿using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Remote.Imaging;
using TableIT.Remote.Messages;

namespace TableIT.Remote.ViewModels
{
    public class ImagesPageViewModel : ObservableObject
    {
        public event Func<Task<string?>>? DisplayPrompt;

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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
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
            IsLoading = true;
            Images = (await ImageManager.LoadImages(true))
                .Select(x => new ImageViewModel(x))
                .ToList();

            foreach(var image in Images)
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(3));
                if (await ImageManager.LoadThumbnailImage(image.Data, cts.Token) is { } remoteImage)
                {
                    image.Image = remoteImage.Thumbnail;
                }
            }
            IsLoading = false;
        }

        private async Task OnImport()
        {
            if (await PickAndShow(PickOptions.Images) is { } fileResult)
            {
                string? fileName = null;
                if (DisplayPrompt is { } displayPrompt)
                {
                    fileName = await displayPrompt();
                }
                fileName ??= fileResult.FileName;
                using Stream stream = await fileResult.OpenReadAsync();
                //TODO: prompt for name
                await ClientManager.GetClient().SendImage(fileName, stream);
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
            catch (Exception)
            {
                // The user canceled or something went wrong
                return null;
            }
        }
    }
}
