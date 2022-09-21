using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace MSHack2022.Analyzers.Blazor;

internal sealed class WellKnownTypes
{
    public INamedTypeSymbol ComponentBase { get; private init; } = default!;
    public IMethodSymbol ComponentBase_SetParametersAsync { get; private init; } = default!;
    public IMethodSymbol ComponentBase_OnInitializedAsync { get; private init; } = default!;
    public INamedTypeSymbol ParameterAttribute { get; private init; } = default!;
    public INamedTypeSymbol CascadingParameterAttribute { get; private init; } = default!;
    public INamedTypeSymbol SupplyParameterFromQueryAttribute { get; private init; } = default!;
    public INamedTypeSymbol EditorRequiredAttribute { get; private init; } = default!;
    public INamedTypeSymbol IJSRuntime { get; private init; } = default!;
    public INamedTypeSymbol EventCallbackFactory { get; private init; } = default!;
    public INamedTypeSymbol RenderTreeBuilder { get; private init; } = default!;

    public static bool TryCreate(Compilation compilation,
        [NotNullWhen(returnValue: true)] out WellKnownTypes? result,
        [NotNullWhen(false)] out string? failedType)
    {
        result = default;

        const string ComponentBase = "Microsoft.AspNetCore.Components.ComponentBase";
        if (compilation.GetTypeByMetadataName(ComponentBase) is not { } componentBase)
        {
            failedType = ComponentBase;
            return false;
        }

        var componentBaseMembers = componentBase.GetMembers();

        const string ComponentBase_SetParametersAsync = "SetParametersAsync";
        if (componentBaseMembers.FirstOrDefault(m => m.MetadataName == ComponentBase_SetParametersAsync) is not IMethodSymbol componentBase_SetParametersAsync)
        {
            failedType = ComponentBase_SetParametersAsync;
            return false;
        }

        const string ComponentBase_OnInitializedAsync = "OnInitializedAsync";
        if (componentBaseMembers.FirstOrDefault(m => m.MetadataName == ComponentBase_OnInitializedAsync) is not IMethodSymbol componentBase_OnInitializedAsync)
        {
            failedType = ComponentBase_OnInitializedAsync;
            return false;
        }

        const string ParameterAttribute = "Microsoft.AspNetCore.Components.ParameterAttribute";
        if (compilation.GetTypeByMetadataName(ParameterAttribute) is not { } parameterAttribute)
        {
            failedType = ParameterAttribute;
            return false;
        }

        const string CascadingParameterAttribute = "Microsoft.AspNetCore.Components.CascadingParameterAttribute";
        if (compilation.GetTypeByMetadataName(CascadingParameterAttribute) is not { } cascadingParameterAttribute)
        {
            failedType = CascadingParameterAttribute;
            return false;
        }

        const string SupplyParameterFromQueryAttribute = "Microsoft.AspNetCore.Components.SupplyParameterFromQueryAttribute";
        if (compilation.GetTypeByMetadataName(SupplyParameterFromQueryAttribute) is not { } supplyParameterFromQueryAttribute)
        {
            failedType = SupplyParameterFromQueryAttribute;
            return false;
        }

        const string EditorRequiredAttribute = "Microsoft.AspNetCore.Components.EditorRequiredAttribute";
        if (compilation.GetTypeByMetadataName(EditorRequiredAttribute) is not { } editorRequiredAttribute)
        {
            failedType = EditorRequiredAttribute;
            return false;
        }

        const string IJSRuntime = "Microsoft.JSInterop.IJSRuntime";
        if (compilation.GetTypeByMetadataName(IJSRuntime) is not { } iJSRuntime)
        {
            failedType = IJSRuntime;
            return false;
        }

        const string EventCallbackFactory = "Microsoft.AspNetCore.Components.EventCallbackFactory";
        if (compilation.GetTypeByMetadataName(EventCallbackFactory) is not { } eventCallbackFactory)
        {
            failedType = EventCallbackFactory;
            return false;
        }

        const string RenderTreeBuilder = "Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder";
        if (compilation.GetTypeByMetadataName(RenderTreeBuilder) is not { } renderTreeBuilder)
        {
            failedType = RenderTreeBuilder;
            return false;
        }

        result = new()
        {
            ComponentBase = componentBase,
            ComponentBase_SetParametersAsync = componentBase_SetParametersAsync,
            ComponentBase_OnInitializedAsync = componentBase_OnInitializedAsync,
            ParameterAttribute = parameterAttribute,
            CascadingParameterAttribute = cascadingParameterAttribute,
            SupplyParameterFromQueryAttribute = supplyParameterFromQueryAttribute,
            EditorRequiredAttribute = editorRequiredAttribute,
            IJSRuntime = iJSRuntime,
            EventCallbackFactory = eventCallbackFactory,
            RenderTreeBuilder = renderTreeBuilder,
        };

        failedType = null;
        return true;
    }
}
