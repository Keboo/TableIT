using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TableIT.Core.Messages;

namespace TableIT.Core.Imaging
{
    internal class ResourceManager
    {
        private SemaphoreSlim ResourcesLock { get; } = new(1, 1);
        private Dictionary<string, CacheResource> Resources { get; } = new();

        private HttpClient HttpClient { get; }


        private IResourcePersistence Persistence { get; }

        public ResourceManager(IResourcePersistence persistence, string endpoint)
        {
            Persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
            HttpClient = new()
            {
                BaseAddress = new Uri(endpoint)
            };
        }

        internal async Task<Stream?> Get(string id, int? width, int? height)
        {
            CacheResource? cached;
            try
            {
                await ResourcesLock.WaitAsync();
                if (Resources.TryGetValue(id, out cached) &&
                    await Persistence.Get(id) is { } dataStream)
                {
                    return dataStream;
                }
            }
            finally
            {
                ResourcesLock.Release();
            }
            var response = await HttpClient.GetAsync($"api/resources/{id}");
            if (response.IsSuccessStatusCode)
            {
                Stream dataStream = await response.Content.ReadAsStreamAsync();
                if (cached is not null)
                {
                    //TODO: response.Headers.ETag;
                    try
                    {
                        await ResourcesLock.WaitAsync();
                        await Persistence.Save(id, dataStream);
                    }
                    finally
                    {
                        ResourcesLock.Release();
                    }
                }
                return dataStream;

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
                    lock (Resources)
                    {
                        foreach (var resource in resources.Where(x => !string.IsNullOrWhiteSpace(x.ResourceId)))
                        {
                            Resources[resource.ResourceId!] = new CacheResource(resource.ResourceId!, resource.Version);
                        }
                    }
                    return resources.Select(r => new ImageData
                    {
                        Id = r.ResourceId,
                        Name = r.DisplayName
                    }).ToList();
                }
            }
            catch (Exception)
            { }

            return Array.Empty<ImageData>();
        }

        private class CacheResource
        {
            public CacheResource(string id, string? version)
            {
                Id = id;
                Version = version;
            }

            public string Id { get; }
            public string? Version { get; }

        }
    }
}
