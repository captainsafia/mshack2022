using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<
    MSHack2022.Analyzers.Blazor.EventCallbackAnalyzer>;

namespace MSHack2022.Tests;

public class EventCallbackAnalyzerTest
{
    [Fact]
    public async Task TriggersInSimpleCase()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

var builder = new RenderTreeBuilder();

for (var i = 0; i < 10; i++)
{
    builder.AddAttribute(
        0,
        ""onclick"",
        EventCallback.Factory.Create(null, () => Console.WriteLine([|i|])));
}
");
    }

    [Fact]
    public async Task TriggersWithMultipleIteratorVariables()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

var builder = new RenderTreeBuilder();

for (int i = 0, j = 0; i < 10 && j < 10; i++, j++)
{
    builder.AddAttribute(
        0,
        ""onclick"",
        EventCallback.Factory.Create(null, () => Console.WriteLine([|j|])));
}
");
    }

    [Fact]
    public async Task DoesNotTriggerWithIteratorDeclaredOutsideForStatement()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

var builder = new RenderTreeBuilder();

var i = 0;
for (i = 0; i < 10; i++)
{
    builder.AddAttribute(
        0,
        ""onclick"",
        EventCallback.Factory.Create(null, () => Console.WriteLine(i)));
}
");
    }

    [Fact]
    public async Task DoesNotTriggerAfterCopyingIntoLocal()
    {
        await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

var builder = new RenderTreeBuilder();

for (var i = 0; i < 10; i++)
{
    var index = i;
    builder.AddAttribute(
        0,
        ""onclick"",
        EventCallback.Factory.Create(null, () => Console.WriteLine(index)));
}
");
    }
}
