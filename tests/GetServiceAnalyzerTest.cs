using Microsoft.CodeAnalysis.Testing;
using MSHack2022.Analyzers;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<
    MSHack2022.Analyzers.GetServiceAnalyzer>;

namespace MSHack2022.Tests;

public partial class GetServiceAnalyzerTest
{
    [Fact]
    public async Task TriggersOnNonGenericGetService()
    {
        await VerifyCS.VerifyAnalyzerAsync(
            """
            using System;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.Hosting;

            var app = WebApplication.Create();

            app.MapGet("/get", {|MH004:(HttpContext context) => 
            {
                var hostEnvironment = (IHostEnvironment)context.RequestServices.GetService(typeof(IHostEnvironment))!;
                return $"Hello from '{hostEnvironment.ApplicationName}'!";
            }|});
            """);
    }

    [Fact]
    public async Task TriggersOnGenericGetService()
    {
        await VerifyCS.VerifyAnalyzerAsync(
            """
            using System;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var app = WebApplication.Create();

            app.MapGet("/get", {|MH004:(HttpContext context) => 
            {
                var hostEnvironment = context.RequestServices.GetService<IHostEnvironment>()!;
                return $"Hello from '{hostEnvironment.ApplicationName}'!";
            }|});
            """);
    }

    [Fact]
    public async Task TriggersOnGenericGetRequiredService()
    {
        await VerifyCS.VerifyAnalyzerAsync(
            """
            using System;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var app = WebApplication.Create();

            app.MapGet("/get", {|MH004:(HttpContext context) => 
            {
                var hostEnvironment = context.RequestServices.GetRequiredService<IHostEnvironment>()!;
                return $"Hello from '{hostEnvironment.ApplicationName}'!";
            }|});
            """);
    }

    [Fact]
    public async Task TriggersOnGenericGetRequiredServiceInMethodGroup()
    {
        await VerifyCS.VerifyAnalyzerAsync(
            """
            using System;
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.Hosting;

            var app = WebApplication.Create();

            app.MapGet("/get", {|MH004:Hello|});

            string Hello(HttpContext context)
            {
                var hostEnvironment = context.RequestServices.GetRequiredService<IHostEnvironment>()!;
                return $"Hello from '{hostEnvironment.ApplicationName}'!";
            }
            """);
    }
}
