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

        [|builder.Services.AddAuthentication().AddJwtBearer()|];

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

[|builder.Services.AddAuthentication().AddJwtBearer()|];

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
        [|services.AddAuthentication().AddJwtBearer()|];
        return services;
    }
}
");
    }
}

