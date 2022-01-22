using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace TableIT.Core
{
    partial class TableClient
    {
        private MessageCache Cache { get; } = new();

        private class MessageCache
        {
            private class CacheItem
            {
                public DateTime Expiration { get; } = DateTime.UtcNow + TimeSpan.FromMinutes(5);

                private List<(int, string)> Items { get; } = new();

                public int Add(int index, string data)
                {
                    lock (Items)
                    {
                        Items.Add((index, data));
                        return Items.Count;
                    }
                }

                public string GetData()
                {
                    StringBuilder sb = new();
                    foreach (var item in Items.OrderBy(t => t.Item1))
                    {
                        sb.Append(item.Item2);
                    }
                    return sb.ToString();
                }
            }

            private Dictionary<Guid, CacheItem> Items { get; } = new();

            public int AddData(Guid id, int index, string data)
            {
                CacheItem? item;
                lock (Items)
                {
                    if (!Items.TryGetValue(id, out item))
                    {
                        Items[id] = item = new CacheItem();
                    }
                }
                return item.Add(index, data);
            }

            public string? GetMessage(Guid id)
            {
                lock (Items)
                {
                    if (Items.TryGetValue(id, out CacheItem? item))
                    {
                        return item.GetData();
                    }
                    foreach (KeyValuePair<Guid, CacheItem> kvp in Items.ToList())
                    {
                        if (kvp.Value.Expiration > DateTime.UtcNow)
                        {
                            Items.Remove(kvp.Key);
                        }
                    }
                }
                return null;
            }

            public T? GetMessage<T>(Guid id) where T : class
            {
                if (GetMessage(id) is { } stringData)
                {
                    return JsonSerializer.Deserialize<T>(stringData);
                }
                return null;
            }
        }
    }
}
