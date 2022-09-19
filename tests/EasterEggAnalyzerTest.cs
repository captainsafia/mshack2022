using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpCodeFixVerifier<
    MSHack2022.Analyzers.EasterEggAnalyzer,
    MSHack2022.Codefixers.EasterEggFixer>;

namespace MSHack2022.Tests;

public class EasterEggAnalyzerTest
{
    [Fact]
    public async Task TriggersOnIntWith42()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
using System;

class Program
{
    static void Main()
    {
        [|int i = 42;|]
    }
}
", @"
using System;

class Program
{
    static void Main()
    {
        int meaningOfLife = 42;
    }
}
");
    }

    [Fact]
    public async Task DoesNotTriggerOnNonIntType()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        string i = ""42"";
    }
}
");
    }

    [Fact]
    public async Task DoesNotTriggerOnIntsOtherThan42()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;

class Program
{
    static void Main()
    {
        int i = 39;
    }
}
");
    }

    [Fact]
    public async Task TriggersOnIntWith42_TopLevelStatements()
    {
        await VerifyCS.VerifyCodeFixAsync(@"
[|int i = 42;|]
", @"
int meaningOfLife = 42;
");
    }
}