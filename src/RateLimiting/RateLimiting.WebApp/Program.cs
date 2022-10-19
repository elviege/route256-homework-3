using RateLimiting.Core.Options;
using RateLimiting.WebApp.Middlewares;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddMvcCore();
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<RateLimitingConfig>(
    builder.Configuration.GetSection("RateLimitingConfig"));

var app = builder.Build();

app.UseRouting();
//app.UseHttpsRedirection();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseEndpoints(b =>
{
    b.MapControllers();
});
//app.MapControllers();

app.Run();