using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text.Json;
using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core.Resources;

internal class ResourceManager
{
    private HttpClient HttpClient { get; }

    private MemoryCache Cache { get; }

    public ResourceManager(string endpoint)
    {
        Cache = MemoryCache.Default;
        HttpClient = new()
        {
            BaseAddress = new Uri(endpoint)
        };
    }

    internal async Task<Stream?> Get(string id, string? version, int? width, int? height)
    {
        string cacheKey = $"{id}{version}{width}{height}";

        if (Cache[cacheKey] is byte[] data)
        {
            return new MemoryStream(data);
        }
        //TODO: retrieve at the specified version?
        try
        {
            string uri = $"api/resources/{id}";
            string query = string.Join("&", GetQueryParameters());
            if (query.Length > 0)
            {
                //uri += "?" + query;
            }
            HttpResponseMessage? response = await HttpClient.GetAsync(uri);
            if (response?.IsSuccessStatusCode == true)
            {
                data = await response.Content.ReadAsByteArrayAsync();
                version = response.Headers.ETag.Tag;
                cacheKey = $"{id}{version}{width}{height}";
                Cache[cacheKey] = data;
                return new MemoryStream(data);
            }
        }
        catch (Exception) { }
        return null;

        IEnumerable<string> GetQueryParameters()
        {
            if (width is not null) yield return $"width={width}";
            if (height is not null) yield return $"height={height}";
        }
    }

    internal async Task<bool> Delete(string id, string? version)
    {
        try
        {
            //TODO: include version?
            string uri = $"api/resources/{id}";
            var response = await HttpClient.DeleteAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
        }
        catch(Exception)
        {
            return false;
        }

        foreach(var kvp in Cache.ToList())
        {
            if (kvp.Key.StartsWith($"{id}"))
            {
                Cache.Remove(kvp.Key);
            }
        }
        return true;
    }

    internal async Task<ImageData?> Import(Stream data, string name)
    {
        using var multipartFormContent = new MultipartFormDataContent();
        //Load the file and set the file's Content-Type header
        var fileStreamContent = new StreamContent(data);
        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        //Add the file
        multipartFormContent.Add(fileStreamContent, name: name, fileName: name);


        //Send it
        var response = await HttpClient.PostAsync("api/import/resources", multipartFormContent);

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            if (JsonSerializer.Deserialize<DisplayResource>(json) is { } resource)
            {
                return new ImageData
                {
                    Id = resource.ResourceId,
                    Name = resource.DisplayName,
                    Version = resource.Version,
                };
            }
        }
        return null;
    }

    //TODO: Dont use Resource class
    internal async Task<IReadOnlyList<ImageData>> GetImages()
    {
        try
        {
            string? response = await HttpClient.GetStringAsync("api/list/resources");
            if (response is not null &&
                JsonSerializer.Deserialize<DisplayResource[]>(response) is DisplayResource[] resources)
            {
                return resources.Select(r => new ImageData
                {
                    Id = r.ResourceId,
                    Name = r.DisplayName,
                    Version = r.Version
                }).ToList();
            }
        }
        catch (Exception)
        { }

        return Array.Empty<ImageData>();
    }
}
