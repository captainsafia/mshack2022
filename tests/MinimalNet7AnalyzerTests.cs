using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<MSHack2022.Analyzers.MinimalNet7Analyzers>;

namespace MSHack2022.Tests;

public class MinimalNet7AnalyzerTests
{
    [Fact]
    public async Task TriggersOnAddJwtBearerCall()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        {|MH003:builder.Services.AddAuthentication().AddJwtBearer()|};

        var app = builder.Build();

        app.Run();
    }
}");
    }

    [Fact]
    public async Task TriggersOnAddJwtBearerCall_TopLevelStatements()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

{|MH003:builder.Services.AddAuthentication().AddJwtBearer()|};

var app = builder.Build();

app.Run();
");
    }

    [Fact]
    public async Task DoesNotTriggerOIncorrectCall_NonExtensionMethodInvocation()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

AddJwtBearer(builder.Services);

var app = builder.Build();

app.Run();

static IServiceCollection AddJwtBearer(IServiceCollection services)
{
    return services;
}
");
    }

    [Fact]
    public async Task DoesNotTriggerOnIncorrectCall_ExtensionMethodInvocation()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddJwtBearer();

        var app = builder.Build();

        app.Run();
    }
}

public static class TestExtensionMethods
{
    public static IServiceCollection AddJwtBearer(this IServiceCollection services)
    {
        return services;
    }
}
");
    }

    [Fact]
    public async Task TriggersOnAddJwtBearerCall_InAnotherMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddJwtBearer();

        var app = builder.Build();

        app.Run();
    }
}

public static class TestExtensionMethods
{
    public static IServiceCollection AddJwtBearer(this IServiceCollection services)
    {
        {|MH003:services.AddAuthentication().AddJwtBearer()|};
        return services;
    }
}
");
    }

    [Fact]
    public async Task TriggersOnRoutesWithSamePrefix()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        {|MH008:app.MapGet(""/mh/001"", () => 
        {
            int i = 42;
            return ""Hello world!"";
        })|};

        {|MH008:app.MapGet(""/mh/002"", () =>
        {
            int i = 42;
            return ""Hello world!"";
        })|};

        app.Run();
    }
}
");
    }

    [Fact]
    public async Task TriggersOnRoutesWithSamePrefix_TopLevelStatements()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

{|MH008:app.MapGet(""/mh/001"", () => 
{
    int i = 42;
    return ""Hello world!"";
})|};

{|MH008:app.MapGet(""/mh/002"", () =>
{
    int i = 42;
    return ""Hello world!"";
})|};

app.Run();
");
    }

    [Fact]
    public async Task TriggersOnRoutesWithSamePrefix_DifferentMethods()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

{|MH008:app.MapGet(""/mh/001"", () => 
{
    return ""Hello world!"";
})|};

{|MH008:app.MapPost(""/mh/002"", () =>
{
    return ""Hello world!"";
})|};

{|MH008:app.MapDelete(""/mh/003"", () =>
{
    return ""Hello world!"";
})|};

app.Run();
");
    }

    [Fact]
    public async Task TriggersOnRoutesWithMultipleSamePrefixes()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

{|MH008:app.MapGet(""/mh/001"", () => 
{
    return ""Hello world!"";
})|};

{|MH008:app.MapPost(""/mh/002"", () =>
{
    return ""Hello world!"";
})|};

{|MH008:app.MapDelete(""/ah/003"", () =>
{
    return ""Hello world!"";
})|};

{|MH008:app.MapGet(""/ah/003"", () =>
{
    return ""Hello world!"";
})|};

app.Run();
");
    }

    [Fact]
    public async Task DoesNotTriggerOnDifferentRoutePrefixes()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet(""/mh/001"", () => 
{
    int i = 42;
    return ""Hello world!"";
});

app.MapGet(""/ah/002"", () =>
{
    int i = 42;
    return ""Hello world!"";
});

app.Run();
");
    }
}

