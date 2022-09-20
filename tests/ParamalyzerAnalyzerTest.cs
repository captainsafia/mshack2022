using Microsoft.CodeAnalysis.Testing;
using MSHack2022.Analyzers;
using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<
    MSHack2022.Analyzers.ParamalyzerAnalyzer>;

namespace MSHack2022.Tests;

public partial class ParamalyzerAnalyzerTest
{
    [Fact]
    public async Task TriggersOnOutParamFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", (out string {|#0:s|}) => 
        {
            s = string.Empty;
            return ""Hello world!"";
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnOutParamFromMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", Out);
    }

    static string Out(out string {|#0:s|})
    {
        s = string.Empty;
        return ""Hello world!"";
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnInParamFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", (in string {|#0:s|}) => 
        {
            return ""Hello world!"";
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnInParamFromMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", Out);
    }

    static string Out(in string {|#0:s|})
    {
        return ""Hello world!"";
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnRefParamFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", (ref string {|#0:s|}) => 
        {
            return ""Hello world!"";
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnRefParamFromMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", Out);
    }

    static string Out(ref string {|#0:s|})
    {
        return ""Hello world!"";
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnMultipleParametersWithModifiersFromMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", Out);
    }

    static string Out(ref string {|#0:s|}, in string {|#1:s2|})
    {
        return ""Hello world!"";
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0),
   new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(1));
    }

    [Fact]
    public async Task TriggersOnMultipleParametersWithModifiersFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", (ref string {|#0:s|}, in string {|#1:s2|}) =>
        {
            return ""Hello world!"";
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(0),
   new DiagnosticResult(DiagnosticDescriptors.BadArgumentModifier).WithLocation(1));
    }

    [Fact]
    public async Task DoesNotTriggersOnNormalParam()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();
        app.MapGet(""/get"", (string s) => 
        {
            return ""Hello world!"";
        });
    }
}
");
    }

    [Fact]
    public async Task TriggersOnByRefReturnFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", () =>
        {
            {|#0:return ""Hello world!"".AsSpan();|}
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.ByRefReturnType).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnByRefReturnFromMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", {|#0:Func|});
    }

    static ReadOnlySpan<char> Func()
    {
        return ""Hello world!"".AsSpan();
    }
}
", new DiagnosticResult(DiagnosticDescriptors.ByRefReturnType).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnAllByRefReturnLocationsFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", () =>
        {
            if (true)
            {
                {|#0:return ""Hello"".AsSpan();|}
            }
            {|#1:return ""Hello world!"".AsSpan();|}
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.ByRefReturnType).WithLocation(0),
   new DiagnosticResult(DiagnosticDescriptors.ByRefReturnType).WithLocation(1));
    }

    [Fact]
    public async Task TriggersOnlyOnByRefReturnLocationFromLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", () =>
        {
            if (true)
            {
                // implicit conversion, user code is fine so don't create diagnostic
                return ""Hello"";
            }
            {|#0:return ""Hello world!"".AsSpan();|}
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.ByRefReturnType).WithLocation(0));
    }

    [Fact]
    public async Task TriggersOnRouteValuesUsageInLambda()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", (HttpRequest req) =>
        {
            return $""{req.{|#0:RouteValues|}[""name""]}"";
        });
    }
}
", new DiagnosticResult(DiagnosticDescriptors.ExplicitRouteValue).WithLocation(0)
        .WithArguments("name"));
    }

    [Fact]
    public async Task TriggersOnRouteValuesUsageInMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

class Program
{
    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet(""/get"", {|#0:Func|});
    }

    static string Func(HttpRequest req)
    {
        return $""{req.RouteValues[""name""]}"";
    }
}
", new DiagnosticResult(DiagnosticDescriptors.ExplicitRouteValue).WithLocation(0)
        .WithArguments("name"));
    }
}