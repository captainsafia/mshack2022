using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpCodeFixVerifier<
    MSHack2022.Analyzers.Blazor.JsInteropAnalyzer,
    MSHack2022.Codefixers.Blazor.JsInteropFixer>;

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
    public async Task OnInitializeAsyncDiagnostic_TriggersWithIJSRuntimeMethod()
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
        await {{|MH007:JS.InvokeAsync<object?>(""console.log"", ""Hello"")|}};
    }}
}}
");
    }

    [Fact]
    public async Task OnInitializeAsyncDiagnostic_TriggersWithIJSRuntimeExtensionMethod_StaticMethodSyntax()
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
        await {{|MH007:JSRuntimeExtensions.InvokeAsync<object>(JS, ""console.log"", ""Hello"")|}};
    }}
}}
");
    }

    [Fact]
    public async Task OnInitializeAsyncDiagnostic_TriggersWithIJSRuntimeExtensionMethod_InstanceMethodSyntax()
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
        await {{|MH007:JS.InvokeVoidAsync(""console.log"", ""Hello"")|}};
    }}
}}
");
    }

    [Fact]
    public async Task OnInitializeAsyncDiagnostic_DoesNotTriggerOutsideOnInitializedAsync()
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

    [Fact]
    public async Task JSInvokableDiagnostic_CodeFixerWorksWithNoAccessModifiers()
    {
        await VerifyCS.VerifyCodeFixAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

public static class JSInvokableMethods
{{
    [JSInvokable]
    static void {{|MH014:DoSomething|}}()
    {{
    }}
}}
", $@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

public static class JSInvokableMethods
{{
    [JSInvokable]
    public static void DoSomething()
    {{
    }}
}}
");
    }

    [Fact]
    public async Task JSInvokableDiagnostic_CodeFixerWorksWithExistingAccessModifiers()
    {
        await VerifyCS.VerifyCodeFixAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

public static class JSInvokableMethods
{{
    [JSInvokable]
    private static void {{|MH014:DoSomething|}}()
    {{
    }}
}}
", $@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

public static class JSInvokableMethods
{{
    [JSInvokable]
    public static void DoSomething()
    {{
    }}
}}
");
    }

    [Fact]
    public async Task JSInvokableDiagnostic_DoesNotTriggerForNonJSInvokableMethods()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

public static class JSInvokableMethods
{{
    private static void DoSomething()
    {{
    }}
}}
");
    }

    [Fact]
    public async Task JSInvokableDiagnostic_DoesNotTriggerForPublicMethods()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

{ProgramMain}

public static class JSInvokableMethods
{{
    [JSInvokable]
    public static void DoSomething()
    {{
    }}
}}
");
    }
}
