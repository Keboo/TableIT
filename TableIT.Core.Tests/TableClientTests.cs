using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        Stream? imageData = await client.GetImage(imageId, "");

        Assert.Equal(bytes.Length, imageData?.Length);
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
            await client.ImportImage("test image", ms);
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

    [Fact]
    public async Task CanGetTableConfiguration()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        string resourceId = Guid.NewGuid().ToString();
        table.Handle<TableConfigurationRequest, TableConfigurationResponse>(message =>
        {
            return Task.FromResult<TableConfigurationResponse?>(new TableConfigurationResponse
            {
                Config = new TableConfiguration
                {
                    Id = table.UserId,
                    CurrentResourceId = resourceId,
                    Compass = new CompassConfiguration
                    {
                        IsShown = true,
                        Size = 42,
                        Color = 0xFF0F0D0E
                    }
                }
            });
        });

        TableConfiguration? config = await client.GetTableConfiguration();

        Assert.NotNull(config);
        Assert.Equal(table.UserId, config!.Id);
        Assert.Equal(resourceId, config.CurrentResourceId);
        Assert.True(config.Compass!.IsShown);
        Assert.Equal(42, config.Compass.Size);
        Assert.Equal(0xFF0F0D0E, config.Compass.Color);

    }

    [Fact]
    public async Task CanSendRotateMessage()
    {
        await using var table = new TableClient();
        await table.StartAsync();
        await using var client = new TableClient(userId: table.UserId);
        await client.StartAsync();

        Task<RotateMessage> getMessage = table.WaitForTableMessage<RotateMessage>();

        await client.SendRotate(90);
        RotateMessage message = await getMessage;

        Assert.Equal(90, message.RotationDegrees);
    }
}

