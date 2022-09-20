using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpCodeFixVerifier<
    MSHack2022.Analyzers.MoveMiddlewareToClassAnalyzer,
    MSHack2022.Codefixers.MoveMiddlewareToClassFixer>;

namespace MSHack2022.Tests;

public partial class MoveMiddlewareToClassAnalyzerTests
{
    [Fact]
    public async Task AnalyzerTriggersOnUse()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

[|app.Use((context, next) => next(context))|];

app.Run();
");
    }

    [Fact]
    public async Task CodefixTriggersOnUse()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

[|app.Use((context, next) => next(context))|];

app.Run();
", @"
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

app.UseMiddleware1();

app.Run();

internal class Middleware1
{
    private readonly RequestDelegate _next;

    public Middleware1(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        return _next(context);
    }
}

internal static class Middleware1Extensions
{
    public static IApplicationBuilder UseMiddleware1(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<Middleware1>();
    }
}
");
    }
}