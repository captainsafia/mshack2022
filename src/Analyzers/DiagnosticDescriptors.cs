using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers;

public static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MeaningOfLife = new(
        "MH001",
        "Found the meaning of life",
        "Found a variable assigned to 42. Consider uing th 'MeaningOfLife' identifier",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}