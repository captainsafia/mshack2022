using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpCodeFixVerifier<
    MSHack2022.Analyzers.WithNameAnalyzer,
    MSHack2022.Codefixers.WithNameFixer>;

namespace MSHack2022.Tests;

public partial class WithNameAnalyzerTest
{
    [Fact]
    public async Task AnalyzerTriggersOnMapGetWithNoParametersReturningListOfT()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

[|app.MapGet(""/todos"", () => todos)|];

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }

    [Fact]
    public async Task AnalyzerDoesNotTriggerOnMapGetAlreadyCallingWithName()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

app.MapGet(""/todos"", () => todos).WithName(""Foo"");

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }

    [Fact]
    public async Task CodeFixTriggersOnMapGetWithNoParametersReturningListOfT()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

[|app.MapGet(""/todos"", () => todos)|];

app.Run();

record Todo(int Id, string Title, bool IsComplete);
", @"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

app.MapGet(""/todos"", () => todos).WithName(""GetTemporaryApiName"");

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }
}