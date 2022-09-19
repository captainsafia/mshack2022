namespace Microsoft.CodeAnalysis;

internal static class IMethodSymbolExtensions
{
    public static bool IsOverriddenBy(this IMethodSymbol methodSymbol, IMethodSymbol? other)
    {
        var currentMethod = other?.OverriddenMethod;

        while (currentMethod is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(methodSymbol, currentMethod))
            {
                return true;
            }

            currentMethod = currentMethod.OverriddenMethod;
        }

        return false;
    }
}
