var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication().AddJwtBearer();

var app = builder.Build();

app.MapGet("/mh001", () => 
{
    int i = 42;
    return "Hello world!";
});

app.Run();
