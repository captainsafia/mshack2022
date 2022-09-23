using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MSHack2022.Codefixers.Extensions;

internal static class ISymbolExtensions
{
    // Get syntax starting from semantic symbol
    public static IEnumerable<SyntaxNode> GetSyntaxNodes(this ISymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(sr => sr.GetSyntax());
    }

    public static bool IsAccessibleOutsideOfAssembly(this ISymbol symbol) =>
        symbol.DeclaredAccessibility switch
        {
            Accessibility.Private => false,
            Accessibility.Internal => false,
            Accessibility.ProtectedAndInternal => false,
            Accessibility.Protected => true,
            Accessibility.ProtectedOrInternal => true,
            Accessibility.Public => true,
            _ => true,
        };
}
