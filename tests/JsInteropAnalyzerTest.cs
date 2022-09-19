using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<
    MSHack2022.Analyzers.Blazor.JsInteropAnalyzer>;

namespace MSHack2022.Tests;

public class JsInteropAnalyzerTest
{
    private static readonly string ProgramMain = @"
class Program
{
    static void Main()
    {
    }
}
";

    [Fact]
    public async Task TriggersWithIJSRuntimeMethod()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Inject]
    private IJSRuntime JS {{ get; set; }}

    protected override async Task OnInitializedAsync()
    {{
        await [|JS.InvokeAsync<object?>(""console.log"", ""Hello"")|];
    }}
}}
");
    }

    [Fact]
    public async Task TriggersWithIJSRuntimeExtensionMethod_StaticMethodSyntax()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Inject]
    private IJSRuntime JS {{ get; set; }}

    protected override async Task OnInitializedAsync()
    {{
        await [|JSRuntimeExtensions.InvokeAsync<object>(JS, ""console.log"", ""Hello"")|];
    }}
}}
");
    }

    [Fact]
    public async Task TriggersWithIJSRuntimeExtensionMethod_InstanceMethodSyntax()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Inject]
    private IJSRuntime JS {{ get; set; }}

    protected override async Task OnInitializedAsync()
    {{
        await [|JS.InvokeVoidAsync(""console.log"", ""Hello"")|];
    }}
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerOutsideOnInitializedAsync()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Inject]
    private IJSRuntime JS {{ get; set; }}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {{
        if (firstRender)
        {{
            await JS.InvokeAsync<object?>(""console.log"", ""Hello"");
        }}
    }}
}}
");
    }
}
