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

    public static readonly DiagnosticDescriptor UseDotnetUserJwtsTool = new(
        "MH003",
        "Recommend using dotnet user-jwts tool",
        "It looks like you're using JWT-bearer based authentication in your application. Consider using the `dotnet user-jwts` tool to generate tokens for local development.",
        "Usage",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);
}