namespace RateLimiting.Core.Options;

public class RateLimitingConfig
{
    public int TimeWindow { get; set; }
    public int MaxRequests { get; set; }
}