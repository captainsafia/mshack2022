using Microsoft.CodeAnalysis;
using System.Diagnostics.CodeAnalysis;

namespace MSHack2022.Analyzers.Blazor;

internal sealed class WellKnownTypes
{
    public INamedTypeSymbol ComponentBase { get; private init; } = default!;
    public INamedTypeSymbol ParameterAttribute { get; private init; } = default!;
    public IMethodSymbol SetParametersAsync { get; private init; } = default!;
    public IMethodSymbol OnInitializedAsync { get; private init; } = default!;
    public INamedTypeSymbol IJSRuntime { get; private init; } = default!;

    public static bool TryCreate(Compilation compilation, [NotNullWhen(returnValue: true)] out WellKnownTypes? result)
    {
        result = default;

        const string ComponentBase = "Microsoft.AspNetCore.Components.ComponentBase";
        if (compilation.GetTypeByMetadataName(ComponentBase) is not { } componentBase)
        {
            return false;
        }

        const string ParameterAttribute = "Microsoft.AspNetCore.Components.ParameterAttribute";
        if (compilation.GetTypeByMetadataName(ParameterAttribute) is not { } parameterAttribute)
        {
            return false;
        }

        const string SetParametersAsync = "SetParametersAsync";
        if (componentBase.GetMembers().FirstOrDefault(m => m.MetadataName == SetParametersAsync) is not IMethodSymbol setParametersAsync)
        {
            return false;
        }

        const string OnInitializedAsync = "OnInitializedAsync";
        if (componentBase.GetMembers().FirstOrDefault(m => m.MetadataName == OnInitializedAsync) is not IMethodSymbol onInitializedAsync)
        {
            return false;
        }

        const string IJSRuntime = "Microsoft.JSInterop.IJSRuntime";
        if (compilation.GetTypeByMetadataName(IJSRuntime) is not { } iJSRuntime)
        {
            return false;
        }

        result = new()
        {
            ComponentBase = componentBase,
            ParameterAttribute = parameterAttribute,
            SetParametersAsync = setParametersAsync,
            OnInitializedAsync = onInitializedAsync,
            IJSRuntime = iJSRuntime,
        };
        return true;
    }
}
