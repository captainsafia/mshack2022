using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MeaningOfLife = new(
        "MH001",
        "Found the meaning of life",
        "Found a variable assigned to 42. Consider using the 'meaningOfLife' identifier.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ComponentsShouldNotWriteToTheirOwnParameters = new(
        "MH002",
        "Components should not write to their own parameters",
        "The parameter '{0}' is being written to from its defining component",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor WithName = new(
        "MH006",
        "Candidate for WithName CodeFix",
        "Add a call to WithName(\"{0}\")",
        "Usage",
        DiagnosticSeverity.Hidden,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UseDotnetUserJwtsTool = new(
        "MH003",
        "Recommend using dotnet user-jwts tool",
        "It looks like you're using JWT-bearer based authentication in your application. Consider using the `dotnet user-jwts` tool to generate tokens for local development.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor GetService = new(
        "MH004",
        "Found call to GetService or GetRequiredService",
        "Found call to GetService or GetRequiredService. Consider using a parameter instead.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BadArgumentModifier = new(
        "MH005",
        "Found an invalid modifier on an argument",
        "Found an argument with an invalid modifier like 'out', 'ref', or 'in'. Remove the modifier.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ShouldNotPerformJsInteropInOnInitializedAsync = new(
        "MH007",
        "JavaScript interop should not be performed within 'OnInitializedAsync()'",
        "The 'OnInitializedAsync()' method may get called during prerendering, when JavaScript " +
        "interop is not available. Please move this call to 'OnAfterRenderAsync(bool firstRender)'.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RecommendUsingRouteGroups = new(
        "MH008",
        "Recommend using route groups",
        "Found several routes that use the same prefix: '{0}'. Consider using a route group to organize these route handlers.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EventCallbackCapturingForLoopIteratorVariable = new(
        "MH009",
        "An EventCallback is capturing a for loop iteration variable",
        "The for loop iteration variable '{0}' is being captured in an EventCallback. " +
        "Consider copying '{0}' into a local variable to avoid this capture.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ByRefReturnType = new(
        "MH011",
        "Found a by-ref return type",
        "Found a by-ref return type. Return a non-reference type instead. e.g. Span<char> is a by-ref type.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ExplicitRouteValue = new(
        "MH012",
        "RouteValues is being used explicitly",
        "RouteValues is being used explicitly. Add a parameter to the method instead. e.g. app.MapGet(\"/{{{0}}}\", (string {0}) => {{}});",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}