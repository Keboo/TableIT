using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core;

public static class TableClientMessageSenders
{
    public static async Task SendPan(this TableClient client, int? horizontalOffset, int? verticalOffset)
    {
        await client.SendTableMessage(new PanMessage
        {
            HorizontalOffset = horizontalOffset,
            VerticalOffset = verticalOffset
        });
    }

    public static async Task SendZoom(this TableClient client, float zoomAdjustment)
    {
        await client.SendTableMessage(new ZoomMessage
        {
            ZoomAdjustment = zoomAdjustment
        });
    }

    public static async Task SendRotate(this TableClient client, int? degrees)
    {
        await client.SendTableMessage(new RotateMessage
        {
            RotationDegrees = degrees
        });
    }

    public static async Task<bool> PingTable(this TableClient client)
    {
        if (await client.SendRequestAsync<TablePingRequest, TablePingResponse>(new TablePingRequest(), TimeSpan.FromSeconds(3)) is not null)
        {
            return true;
        }
        return false;
    }

    public static async Task<TableConfiguration?> GetTableConfiguration(this TableClient client)
    {
        if (await client.SendRequestAsync<TableConfigurationRequest, TableConfigurationResponse>(new()) is { } response)
        {
            return response.Config;
        }
        return null;
    }

    public static async Task UpdateTableConfiguration(this TableClient client, TableConfiguration config)
    {
        await client.SendTableMessage(new SetTableConfigurationMessage
        {
            Config = config
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="client"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private static async Task SendTableMessage<TMessage>(this TableClient client, TMessage message)
        => await client.SendAsync("tablemessage", message);

    public static void RegisterTableMessage<TMessage>(this TableClient client, Action<TMessage> handler)
        where TMessage : class
        => client.Register<TMessage>(handler);

    private static async Task SendRemoteMessage<TMessage>(this TableClient client, TMessage message)
        => await client.SendAsync("remotemessage", message);

    public static async Task SetCurrentImage(this TableClient client, string imageId)
    {
        //TODO: Include version
        await client.SendTableMessage(new LoadImageMessage
        {
            ImageId = imageId,
        });
    }

    public static string GetImageUrl(this TableClient client, string imageId, int? width = null, int? height = null)
    {
        if (string.IsNullOrWhiteSpace(imageId)) return "";

        string baseUrl = $"{client.Endpoint}/api/resources/{imageId}";
        string query = string.Join("&", GetQueryParamter());
        if (query.Length > 0)
        {
            baseUrl += "?" + query;
        }
        return baseUrl;

        IEnumerable<string> GetQueryParamter()
        {
            if (width is not null)
                yield return $"width={width}";
            if (height is not null)
                yield return $"height={height}";
        }
    }
}
