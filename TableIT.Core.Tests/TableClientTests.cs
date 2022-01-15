using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;
using Xunit;

namespace TableIT.Core.Tests;

public class TableClientTests
{
    [Fact]
    public async Task CanSendZoomMessage()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        Task<ZoomMessage> getMessage = table.WaitForMessage<ZoomMessage>();

        await client.SendZoom(0.42f);
        ZoomMessage message = await getMessage;

        Assert.Equal(0.42f, message.ZoomAdjustment);
    }

    [Fact]
    public async Task CanSendPanMessage()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        Task<PanMessage> getMessage = table.WaitForMessage<PanMessage>();

        await client.SendPan(-5, 12_000);
        PanMessage message = await getMessage;

        Assert.Equal(-5, message.HorizontalOffset);
        Assert.Equal(12_000, message.VerticalOffset);
    }

    [Fact]
    public async Task CanListImages()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        Guid imageId = Guid.NewGuid();
        string imageName = "test image name";
        table.Handle<ListImagesRequest, ListImagesResponse>(message =>
        {
            return Task.FromResult<ListImagesResponse?>(new ListImagesResponse
            {
                Images = new()
                {
                    new ImageData
                    {
                        Id = imageId,
                        Name = imageName
                    }
                }
            });
        });

        IReadOnlyList<ImageData> images = await client.GetImages();

        Assert.Equal(1, images.Count);
        Assert.Equal(imageId, images[0].Id);
        Assert.Equal(imageName, images[0].Name);
    }

    [Fact]
    public async Task CanRetrieveImageData()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        var random = new Random();
        //Big enough to cause multiple messages
        byte[] bytes = new byte[12_163];
        random.NextBytes(bytes);

        Guid imageId = Guid.NewGuid();
        table.Handle<GetImageRequest, GetImageResponse>(message =>
        {
            if (message.ImageId == imageId)
            {
                return Task.FromResult<GetImageResponse?>(new GetImageResponse
                {
                    Base64Data = Convert.ToBase64String(bytes),
                });
            }
            return Task.FromResult<GetImageResponse?>(null);
        });

        byte[] imageData = await client.GetImage(imageId);

        Assert.Equal(bytes.Length, imageData.Length);
        Assert.Equal(bytes, imageData);
    }
}

