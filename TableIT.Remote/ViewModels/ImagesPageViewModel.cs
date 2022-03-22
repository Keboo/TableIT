using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Essentials;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TableIT.Core;
using TableIT.Remote.Imaging;
using TableIT.Remote.Messages;

namespace TableIT.Remote.ViewModels;

public class ImagesPageViewModel : ObservableObject
{
    public event Func<Task<string?>>? DisplayPrompt;

    public IRelayCommand SelectCommand { get; }
    public IRelayCommand ImportCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; }
    public TableClientManager ClientManager { get; }
    public IImageManager ImageManager { get; }
    public IMessenger Messenger { get; }
    public IDispatcher Dispatcher { get; }
    public ObservableCollection<ImageViewModel> Images { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public ImagesPageViewModel(
        TableClientManager clientManager,
        IImageManager imageManager,
        IMessenger messenger, 
        IDispatcher dispatcher)
    {
        ImportCommand = new AsyncRelayCommand(OnImport);
        RefreshCommand = new AsyncRelayCommand(async () => await LoadImages(true));
        DeleteCommand = new AsyncRelayCommand<ImageViewModel>(DeleteImage);
        SelectCommand = new AsyncRelayCommand<ImageViewModel>(OnSelect);
        ClientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        ImageManager = imageManager ?? throw new ArgumentNullException(nameof(imageManager));
        Messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
        Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public async Task LoadImages(bool force)
    {
        IsLoading = true;

        if (force || Images.Count == 0)
        {
            var images = (await ImageManager.LoadImages(true))
                .Select(x => new ImageViewModel(x))
                .OrderBy(vm => vm.Name)
                .ToList();
            Images.Clear();
            foreach (var image in images)
            {
                Images.Add(image);
            }

            await Task.WhenAll(images.Select(LoadThumbnail));
        }
        IsLoading = false;
    }

    private async Task LoadThumbnail(ImageViewModel image)
    {
        if (await ImageManager.LoadThumbnailImage(image.Data) is { } remoteImage)
        {
            image.Image = remoteImage.Thumbnail;
        }
    }

    private async Task OnImport()
    {
        try
        {
            if (await PickAndShow(PickOptions.Images) is { } fileResult)
            {
                string? imageName = null;
                if (DisplayPrompt is { } displayPrompt)
                {
                    imageName = await displayPrompt();
                }
                imageName ??= fileResult.FileName;
                
                using Stream stream = await fileResult.OpenReadAsync();
                if (ClientManager.GetClient() is { } client &&
                    await client.ImportImage(imageName, stream) is { } imageData)
                {
                    var imageViewModel = new ImageViewModel(new RemoteImage(imageData.Id, imageData.Name, imageData.Version));
                    Images.Add(imageViewModel);
                    await LoadThumbnail(imageViewModel);
                }
            }
        }
        catch(Exception)
        {

        }
    }

    public void OnItemSelected(ImageViewModel? imageViewModel)
    {
        if (imageViewModel is null) return;
        Messenger.Send(new ImageSelected(imageViewModel.Data.ImageId));
    }

    private async Task OnSelect(ImageViewModel? imageViewModel)
    {
        if (imageViewModel is not null &&
            ClientManager.GetClient() is { } client)
        {
            await client.SetCurrentImage(imageViewModel.Data.ImageId);
        }
    }

    private async Task DeleteImage(ImageViewModel? image)
    {
        if (image is null) return;
        if (ClientManager.GetClient() is { } client &&
            await client.DeleteImage(image.Data.ImageId, image.Data.Version))
        {
            await Dispatcher.DispatchAsync(() =>
            {
                Images.Remove(image);
            });
        }
    }

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
