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
    public INamedTypeSymbol EventCallbackFactory { get; private init; } = default!;

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

        const string ParameterAttribute = "Microsoft.AspNetCore.Components.ParameterAttribute";
        if (compilation.GetTypeByMetadataName(ParameterAttribute) is not { } parameterAttribute)
        {
            failedType = ParameterAttribute;
            return false;
        }

        const string SetParametersAsync = "SetParametersAsync";
        if (componentBase.GetMembers().FirstOrDefault(m => m.MetadataName == SetParametersAsync) is not IMethodSymbol setParametersAsync)
        {
            failedType = SetParametersAsync;
            return false;
        }

        const string OnInitializedAsync = "OnInitializedAsync";
        if (componentBase.GetMembers().FirstOrDefault(m => m.MetadataName == OnInitializedAsync) is not IMethodSymbol onInitializedAsync)
        {
            failedType = OnInitializedAsync;
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

        result = new()
        {
            ComponentBase = componentBase,
            ParameterAttribute = parameterAttribute,
            SetParametersAsync = setParametersAsync,
            OnInitializedAsync = onInitializedAsync,
            IJSRuntime = iJSRuntime,
            EventCallbackFactory = eventCallbackFactory,
        };

        failedType = null;
        return true;
    }
}
