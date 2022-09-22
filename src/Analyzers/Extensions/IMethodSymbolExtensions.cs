using System.Collections.Immutable;
using System.Diagnostics;

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

    // https://sourceroslyn.io/#Microsoft.CodeAnalysis.Workspaces/ISymbolExtensions.cs,93
    public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(this ISymbol symbol)
    {
        if (symbol.Kind != SymbolKind.Method && symbol.Kind != SymbolKind.Property && symbol.Kind != SymbolKind.Event)
            return ImmutableArray<ISymbol>.Empty;

        var containingType = symbol.ContainingType;
        var query = from iface in containingType.AllInterfaces
                    from interfaceMember in iface.GetMembers()
                    let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                    where symbol.Equals(impl, SymbolEqualityComparer.Default)
                    select interfaceMember;
        return query.ToImmutableArray();
    }

    public static bool TryGetBaseInterfaceMethod(this IMethodSymbol method, out IMethodSymbol? baseMethod)
    {
        baseMethod = default;

        foreach (var i in method.ContainingType.AllInterfaces.Reverse()) // Reverse topological order
        {
            var interfaceMethod = i.GetMembers().FirstOrDefault(member => member is IMethodSymbol methodSymbol && methodSymbol.Name == method.Name) as IMethodSymbol;
            if (interfaceMethod != null)
            {
                baseMethod = interfaceMethod;
                return true;
            }
        }

        return false;
    }
}
