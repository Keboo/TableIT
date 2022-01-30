using System;
using System.Collections.Generic;
using System.Text;

namespace TableIT.Core.Messages;

public class TableConfigurationRequest
{ }

public class TableConfigurationResponse
{
    public TableConfiguration? Config { get; set; }
}

public class SetTableConfigurationMessage
{
    //TODO: might want to change this since not all are setable
    public TableConfiguration? Config { get; set; }
}

public class TableConfiguration
{
    public string? Id { get; set; }
    public string? CurrentResourceId { get; set; }
    public CompassConfiguration? Compass { get; set; }
}

public class CompassConfiguration
{
    public bool IsShown { get; set; }
    public int Size { get; set; }
    /// <summary>
    /// Color in ARGB
    /// </summary>
    public uint Color { get; set; }
}
