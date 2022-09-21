var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();

var app = builder.Build();

app.MapGet("/mh/001", () => 
{
    int i = 42;
    return "Hello world!";
});

app.MapGet("/mh/004", (HttpContext context) =>
{
    var hostEnvironment = context.RequestServices.GetRequiredService<IHostEnvironment>();
    return $"Hello from application '{hostEnvironment.ApplicationName}'!";
});

app.MapGet("/mh/005", (string s) =>
{
    return "Hello world!";
});

app.Run();
