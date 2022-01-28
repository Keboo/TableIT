﻿namespace TableIT.Core;

public class ResourceData
{
    public string Id { get; }
    public string Version { get; }

    public ResourceData(string id, string version)
    {
        Id = id;
        Version = version;
    }
}