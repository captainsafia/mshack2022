using Microsoft.Extensions.DependencyInjection;
using WebSample.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();
var hcsBuilder = builder.Services.AddHealthChecks();

hcsBuilder.AddCheck<GetValuesHealthCheck>("getvalueshc");

var app = builder.Build();

app.MapGet("/mh001", () =>
{
    int i = 42;
    return "Hello world!";
});

app.MapGet("/mh004", (HttpContext context) =>
{
    var hostEnvironment = context.RequestServices.GetRequiredService<IHostEnvironment>();
    return $"Hello from application '{hostEnvironment.ApplicationName}'!";
});

app.Run();
