using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MeaningOfLife = new(
        "MH001",
        "Found the meaning of life",
        "Found a variable assigned to 42. Consider using the 'meaningOfLife' identifier",
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
}