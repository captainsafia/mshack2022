using System.Threading.Tasks;
using Xunit;
using VerifyCS = MSHack2022.Tests.CSharpAnalyzerVerifier<
    MSHack2022.Analyzers.Blazor.ComponentParameterAnalyzer>;

namespace MSHack2022.Tests;

public class ComponentParameterAnalyzerTest
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
    public async Task TriggersInComponentClass_SimpleAssignment()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }}

    public void Bar()
    {{
        [|Foo = 5|];
    }}
}}
");
    }

    [Fact]
    public async Task TriggersInComponentClass_CompoundAssignment()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }}

    public void Bar()
    {{
        [|Foo += 5|];
    }}
}}
");
    }

    [Fact]
    public async Task TriggersInComponentClass_CoalesceAssignment()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public object Foo {{ get; set; }}

    public void Bar()
    {{
        [|Foo ??= new()|];
    }}
}}
");
    }

    [Fact]
    public async Task TriggerInComponentClass_InheritFromExistingComponent()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }}
}}

class MyDerivedComponent : MyComponent
{{
    public void Bar()
    {{
        [|Foo = 5|];
    }}
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerInComponentClass_NonParameterProperty()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    public int Foo {{ get; set; }}

    public void Bar()
    {{
        Foo = 5;
    }}
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerInComponentClass_InlineAssignment()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }} = 5;
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerInComponentClass_AssignmentInConstructor()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }}

    public MyComponent()
    {{
        Foo = 5;
    }}
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerInComponentClass_AssignmentInSetParametersAsync()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }}

    public override Task SetParametersAsync(ParameterView parameters)
    {{
        Foo = 5;
        return base.SetParametersAsync(parameters);
    }}
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerInComponentClass_AssignmentInSetParametersAsync_InheritFromExistingComponent()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    public int Foo {{ get; set; }}

    public override Task SetParametersAsync(ParameterView parameters)
        => base.SetParametersAsync(parameters);
}}

class MyDerivedComponent : MyComponent
{{
    public override Task SetParametersAsync(ParameterView parameters)
    {{
        Foo = 5;
        return base.SetParametersAsync(parameters);
    }}
}}
");
    }

    [Fact]
    public async Task DoesNotTriggerInNonComponentClass()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent
{{
    [Parameter]
    public int Foo {{ get; set; }}

    public void Bar()
    {{
        Foo = 5;
    }}
}}
");
    }
}
