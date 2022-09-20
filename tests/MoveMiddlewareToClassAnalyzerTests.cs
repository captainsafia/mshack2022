using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<
    MSHack2022.Analyzers.MoveMiddlewareToClassAnalyzer>;

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
}