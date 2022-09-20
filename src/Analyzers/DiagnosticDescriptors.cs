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
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ShouldNotPerformJsInteropInOnInitializedAsync = new(
        "MH007",
        "JavaScript interop should not be performed within 'OnInitializedAsync()'",
        "The 'OnInitializedAsync()' method may get called during prerendering, when JavaScript " +
        "interop is not available. Please move this call to 'OnAfterRenderAsync(bool firstRender)'.",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}