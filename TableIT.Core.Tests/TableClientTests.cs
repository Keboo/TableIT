using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        Task<ZoomMessage> getMessage = table.WaitForTableMessage<ZoomMessage>();

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

        Task<PanMessage> getMessage = table.WaitForTableMessage<PanMessage>();

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

        IReadOnlyList<ImageData> images = await client.GetImages();

        Assert.True(images.Any());
        Assert.False(string.IsNullOrWhiteSpace(images[0].Id));
        Assert.False(string.IsNullOrWhiteSpace(images[0].Name));
    }

    [Fact(Skip = "Needs update")]
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

        string imageId = Guid.NewGuid().ToString();
        //table.Handle<GetImageRequest, GetImageResponse>(message =>
        //{
        //    if (message.ImageId == imageId)
        //    {
        //        return Task.FromResult<GetImageResponse?>(new GetImageResponse
        //        {
        //            Base64Data = Convert.ToBase64String(bytes),
        //        });
        //    }
        //    return Task.FromResult<GetImageResponse?>(null);
        //});

        byte[] imageData = await client.GetImage(imageId);

        Assert.Equal(bytes.Length, imageData.Length);
        Assert.Equal(bytes, imageData);
    }

    [Fact(Skip = "Needs update")]
    public async Task CanSendImageData()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        var random = new Random();
        //Big enough to cause multiple messages
        byte[] bytes = new byte[12_163];
        random.NextBytes(bytes);

        Task<LoadImageMessage> getMessage = table.WaitForTableMessage<LoadImageMessage>();

        using (var ms = new MemoryStream(bytes))
        {
            await client.SendImage("test image", ms, Guid.NewGuid().ToString());
        }
        LoadImageMessage message = await getMessage;

        //byte[] imageData = Convert.FromBase64String(message.Base64Data ?? "");
        //Assert.Equal(bytes.Length, imageData.Length);
        //Assert.Equal(bytes, imageData);
        //Assert.Equal("test image", message.ImageName);
    }

    [Fact]
    public async Task CanSetImage()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        Task<SetImageMessage> getMessage = table.WaitForTableMessage<SetImageMessage>();

        string imageId = Guid.NewGuid().ToString();
        await client.SetCurrentImage(imageId);
        SetImageMessage message = await getMessage;

        Assert.Equal(imageId, message.ImageId);
    }

    [Fact]
    public async Task CanPingTable_WithExistingTable()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        table.Handle<TablePingRequest, TablePingResponse>(message =>
        {
            return Task.FromResult<TablePingResponse?>(new TablePingResponse());
        });

        bool ack = await client.PingTable();

        Assert.True(ack);
    }

    [Fact]
    public async Task CanPingTable_NoTable()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        client.Timeout = TimeSpan.FromSeconds(3);
        await client.StartAsync();

        bool ack = await client.PingTable();

        Assert.False(ack);
    }

    [Fact(Skip = "Needs update")]
    public async Task TestPerf()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        void Table_ConnectionStateChanged(object? sender, EventArgs e)
        {
            Debug.WriteLine($"TABLE: Connection State changed {table.ConnectionState}");
        }
        table.ConnectionStateChanged += Table_ConnectionStateChanged;
        await using var client = new TableClient(userId: table.UserId);
        //await using var client = new TableClient(userId: "DEBUG2");
        await client.StartAsync();

        const int numImages = 4;

        var random = new Random();


        var images = Enumerable.Range(0, numImages)
            .Select(_ => Guid.NewGuid().ToString())
            .ToDictionary(x => x, _ => random.Next(1_000_000, 2_000_000));

        table.Handle<ListImagesRequest, ListImagesResponse>(message =>
        {
            return Task.FromResult<ListImagesResponse?>(new ListImagesResponse
            {
                Images = images.Select(kvp => new ImageData
                {
                    Id = kvp.Key,
                    Name = kvp.Key.ToString(),
                }).ToList()
            });
        });
        //table.Handle<GetImageRequest, GetImageResponse>(message =>
        //{
        //    Debug.WriteLine($"{DateTime.Now.TimeOfDay} TABLE: got image request {message}");

        //    if (images.TryGetValue(message.ImageId, out int imageSize))
        //    {
        //        byte[] bytes = new byte[imageSize];
        //        random.NextBytes(bytes);
        //        return Task.FromResult<GetImageResponse?>(new GetImageResponse
        //        {
        //            Base64Data = Convert.ToBase64String(bytes),
        //        });
        //    }
        //    return Task.FromResult<GetImageResponse?>(null);
        //});

        IReadOnlyList<ImageData> imageDatas = await client.GetImages();

        Assert.Equal(numImages, imageDatas.Count);

        //var tasks = imageDatas.Select(imageData => client.GetImage(imageData.Id, 100));
        try
        {
            foreach (var imageData in imageDatas)
            {
                byte[] imageBytes = await client.GetImage(imageData.Id, 100);
                Assert.Equal(images[imageData.Id], imageBytes.Length);
            }
        }
        catch (Exception)
        {
            throw;
        }
        
    }
}

