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
    public class ImagesPageViewModel : ObservableObject
    {
        public IRelayCommand ImportCommand { get; }
        public TableClientManager ClientManager { get; }

        private IReadOnlyList<ImageViewModel>? _images;
        public IReadOnlyList<ImageViewModel>? Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        public ImagesPageViewModel(TableClientManager clientManager)
        {
            ImportCommand = new AsyncRelayCommand(OnImport);
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

        private async Task OnImport()
        {
            if (await PickAndShow(PickOptions.Images) is { } fileResult)
            {
                using Stream stream = await fileResult.OpenReadAsync();

                await ClientManager.GetClient().SendImage(stream);
            }
        }

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
