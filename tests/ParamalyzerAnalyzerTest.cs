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
    public async Task DoesNotTriggersOnNormalParam()
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

        app.MapGet(""/get"", (string s) => 
        {
            return ""Hello world!"";
        });
    }
}
");
    }
}