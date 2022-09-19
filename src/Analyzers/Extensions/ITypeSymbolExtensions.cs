namespace Microsoft.CodeAnalysis;

internal static class ITypeSymbolExtensions
{
    public static bool IsBaseTypeOf(this ITypeSymbol typeSymbol, ITypeSymbol? other)
    {
        var currentType = other?.BaseType;

        while (currentType is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(typeSymbol, currentType))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }
}
