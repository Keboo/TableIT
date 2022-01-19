using System;

namespace TableIT.Core.Messages
{
    public class EnvelopeResponse
    {
        public string? MethodName { get; set; }
        public Type? DataType { get; set; }
        public string? Data { get; set; }
    }
}
