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
    public async Task WriteDiagnostic_TriggersInComponentClass_SimpleAssignment()
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
        {{|MH002:Foo = 5|}};
    }}
}}
");
    }

    [Fact]
    public async Task WriteDiagnostic_TriggersInComponentClass_CompoundAssignment()
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
        {{|MH002:Foo += 5|}};
    }}
}}
");
    }

    [Fact]
    public async Task WriteDiagnostic_TriggersInComponentClass_CoalesceAssignment()
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
        {{|MH002:Foo ??= new()|}};
    }}
}}
");
    }

    [Fact]
    public async Task WriteDiagnostic_TriggerInComponentClass_InheritFromExistingComponent()
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
        {{|MH002:Foo = 5|}};
    }}
}}
");
    }

    [Fact]
    public async Task WriteDiagnostic_DoesNotTriggerInComponentClass_NonParameterProperty()
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
    public async Task WriteDiagnostic_DoesNotTriggerInComponentClass_InlineAssignment()
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
    public async Task WriteDiagnostic_DoesNotTriggerInComponentClass_AssignmentInConstructor()
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
    public async Task WriteDiagnostic_DoesNotTriggerInComponentClass_AssignmentInSetParametersAsync()
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
    public async Task WriteDiagnostic_DoesNotTriggerInComponentClass_AssignmentInSetParametersAsync_InheritFromExistingComponent()
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
    public async Task WriteDiagnostic_DoesNotTriggerInNonComponentClass()
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

    [Fact]
    public async Task MissingAttributeDiagnostic_TriggersForSupplyParameterFromQueryAttribute()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [{{|MH013:SupplyParameterFromQuery|}}]
    public int Foo {{ get; set; }}
}}
");
    }

    [Fact]
    public async Task MissingAttributeDiagnostic_TriggersForEditorRequiredAttribute()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [{{|MH013:EditorRequired|}}]
    public int Foo {{ get; set; }}
}}
");
    }

    [Fact]
    public async Task MissingAttributeDiagnostic_DoesNotTriggerForOtherAttributes()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;
using System;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Obsolete]
    public int Foo {{ get; set; }}
}}
");
    }

    [Fact]
    public async Task MissingAttributeDiagnostic_DoesNotTriggerWhenParameterAttributeIsPresent()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [Parameter]
    [SupplyParameterFromQuery]
    public int Foo {{ get; set; }}
}}
");
    }

    [Fact]
    public async Task MissingAttributeDiagnostic_DoesNotTriggerWhenCascadingParameterAttributeIsPresent()
    {
        await VerifyCS.VerifyAnalyzerAsync($@"
using Microsoft.AspNetCore.Components;

{ProgramMain}

class MyComponent : ComponentBase
{{
    [CascadingParameter]
    [SupplyParameterFromQuery]
    public int Foo {{ get; set; }}
}}
");
    }
}
