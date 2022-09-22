using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis;

internal static class ISymbolExtensions
{
    // Get syntax starting from semantic symbol
    public static IEnumerable<SyntaxNode> GetSyntaxNodes(this ISymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences.Select(sr => sr.GetSyntax());
    }
}
