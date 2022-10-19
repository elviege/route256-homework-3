namespace RateLimiting.Core.Attributes;

public class LimitRequests : Attribute
{
    public int TimeWindow { get; set; }
    public int MaxRequests { get; set; }
    public string RemoteIpAddress { get; set; }
}