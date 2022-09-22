using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpCodeRefactoringVerifier<MSHack2022.Codefixers.RouteGroupsCodeRefactoringProvider>;

namespace MSHack2022.Tests;

public class RouteGroupCodeRefactoringTests
{
    [Fact]
    public async Task CanRefactorRouteGroupsWithSamePrefix()
    {
        await VerifyCS.VerifyRefactoringAsync(@"
using Microsoft.AspNetCore.Builder;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();

        $$app.MapGet(""/mh/001"", () => 
        {
            int i = 42;
            return ""Hello world!"";
        });

        app.MapGet(""/mh/002"", () =>
        {
            int i = 42;
            return ""Hello world!"";
        });

        app.Run();
    }
}
", @"
using Microsoft.AspNetCore.Builder;

public static class Program
{
    static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var app = builder.Build();
        var mh = app.MapGroup(""/mh"");

        mh.MapGet(""/001"", () => 
        {
            int i = 42;
            return ""Hello world!"";
        });

        mh.MapGet(""/002"", () =>
        {
            int i = 42;
            return ""Hello world!"";
        });

        app.Run();
    }
}
");
    }
}
