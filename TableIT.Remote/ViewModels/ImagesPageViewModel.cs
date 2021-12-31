using Microsoft.Maui.Essentials;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Core.Messages;

namespace TableIT.Remote.ViewModels
{
    public class ImagesPageViewModel : ObservableObject
    {
        private TableClient Client { get; }
        
        public IRelayCommand PickImage { get; }
        public TableClientManager ClientManager { get; }

        public ImagesPageViewModel(TableClientManager clientManager)
        {
            PickImage = new AsyncRelayCommand(OnPickImage);
            ClientManager = clientManager;
        }

        public async Task LoadImages()
        {
            if (ClientManager.GetClient() is { } client)
            {
                var response = await client.SendRequestAsync<ListImagesRequest, ListImagesResponse>(new ListImagesRequest());
            }
        }

        private async Task OnPickImage()
        {
            if (await PickAndShow(PickOptions.Images) is { } fileResult)
            {
                using Stream stream = await fileResult.OpenReadAsync();

                await Client.SendImage(stream);
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
