using Microsoft.AspNetCore.Mvc;
using RateLimiting.Core.Attributes;

namespace RateLimiting.WebApp.Controllers;

[Route("test")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetTest()
    {
        return Ok("Hello World!");
    }
    
    [HttpGet("elvina")]
    [LimitRequests(MaxRequests = 5, TimeWindow = 5, RemoteIpAddress = "::1")]
    public async Task<ActionResult<string>> GetElvina()
    {
        return Ok("Hello Elvina!");
    }
}