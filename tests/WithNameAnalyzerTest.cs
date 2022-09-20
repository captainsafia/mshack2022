using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpCodeFixVerifier<
    MSHack2022.Analyzers.WithNameAnalyzer,
    MSHack2022.Codefixers.WithNameFixer>;

namespace MSHack2022.Tests;

public partial class WithNameAnalyzerTest
{
    [Fact]
    public async Task AnalyzerTriggersOnMapGetWithNoParameters()
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
    public async Task AnalyzerTriggersOnMapPostWithNoParameters()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

[|app.MapPost(""/todos"", () => {})|];

app.Run();
");
    }

    [Fact]
    public async Task CodefixTriggersOnMapPostWithNoParameters()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

[|app.MapPost(""/todos"", () => {})|];

app.Run();
", @"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

app.MapPost(""/todos"", () => {}).WithName(""CreateTodo"");

app.Run();
");
    }

    [Fact]
    public async Task CodefixTriggersOnMapPutWithNoParameters()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

[|app.MapPut(""/todos"", () => {})|];

app.Run();
", @"
using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

app.MapPut(""/todos"", () => {}).WithName(""UpdateTodo"");

app.Run();
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
    public async Task AnalyzerDoesNotTriggerOnMapGetWithEndpointNameAttribute()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

app.MapGet(""/todos"", [EndpointName(""Foo"")]() => todos);

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }

    [Fact]
    public async Task CodeFixOnMapGet_SuggestsNameUsing_VerbAndRoutePattern()
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

app.MapGet(""/todos"", () => todos).WithName(""GetTodos"");

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }

    [Fact]
    public async Task CodeFixOnMapGet_SuggestsNameUsing_VerbAndRoutePattern_MultipleSegments()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

[|app.MapGet(""/apis/todos"", () => todos)|];

app.Run();

record Todo(int Id, string Title, bool IsComplete);
", @"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

app.MapGet(""/apis/todos"", () => todos).WithName(""GetTodos"");

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }

    [Fact]
    public async Task CodeFixOnMapGet_SuggestsNameUsing_VerbAndRoutePatternWithParameter()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

[|app.MapGet(""/todos/{id}"", () => todos)|];

app.Run();

record Todo(int Id, string Title, bool IsComplete);
", @"
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create();

var todos = new List<Todo> { new(1, ""Do the laundry"", false) };

app.MapGet(""/todos/{id}"", () => todos).WithName(""GetTodosById"");

app.Run();

record Todo(int Id, string Title, bool IsComplete);
");
    }
}
