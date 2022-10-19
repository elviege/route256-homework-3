using System.Net;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using RateLimiting.Core.Attributes;
using RateLimiting.Core.Extensions;
using RateLimiting.Core.Models;
using RateLimiting.Core.Options;

namespace RateLimiting.WebApp.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingConfig _options;
    private readonly IDistributedCache _cache;

    public RateLimitingMiddleware(RequestDelegate next, 
        IOptions<RateLimitingConfig> options, IDistributedCache cache)
    {
        _next = next;
        _options = options.Value;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint(); //всегда null! почему?
        var rateLimitParams = endpoint?.Metadata.GetMetadata<LimitRequests>();

        var currentRemoteIpAddress = context.Connection.RemoteIpAddress.ToString();
        if (rateLimitParams is null || rateLimitParams.RemoteIpAddress != currentRemoteIpAddress)
        {
            rateLimitParams = new LimitRequests
            {
                MaxRequests = _options.MaxRequests,
                TimeWindow = _options.TimeWindow
            };
        }

        var key = GenerateClientKey(context);
        var clientStatistics = await GetClientStatisticsByKey(key);

        if (clientStatistics != null 
            && DateTime.UtcNow < clientStatistics.LastSuccessfulResponseTime.AddSeconds(rateLimitParams.TimeWindow) 
            && clientStatistics.NumberOfRequestsCompletedSuccessfully == rateLimitParams.MaxRequests)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            return;
        }

        await UpdateClientStatisticsStorage(key, rateLimitParams.MaxRequests);
        await _next(context);
    }
    
    private async Task<ClientStatistics> GetClientStatisticsByKey(string key)
    {   
        return await _cache.GetCacheValueAsync<ClientStatistics>(key);
    }

    private static string GenerateClientKey(HttpContext context)
    {
        return $"{context.Request.Path}_{context.Connection.RemoteIpAddress}";
    }

    private async Task UpdateClientStatisticsStorage(string key, int maxRequests)
    {
        var clientStat = await _cache.GetCacheValueAsync<ClientStatistics>(key);

        if (clientStat != null)
        {
            clientStat.LastSuccessfulResponseTime = DateTime.UtcNow;

            if (clientStat.NumberOfRequestsCompletedSuccessfully == maxRequests)
                clientStat.NumberOfRequestsCompletedSuccessfully = 1;
            else
                clientStat.NumberOfRequestsCompletedSuccessfully++;

            await _cache.SetCahceValueAsync<ClientStatistics>(key, clientStat);
        }
        else
        {
            var clientStatistics = new ClientStatistics
            {
                LastSuccessfulResponseTime = DateTime.UtcNow,
                NumberOfRequestsCompletedSuccessfully = 1
            };

            await _cache.SetCahceValueAsync<ClientStatistics>(key, clientStatistics);
        }

    }
}