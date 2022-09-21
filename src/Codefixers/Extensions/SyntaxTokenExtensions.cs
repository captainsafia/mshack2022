using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis;

internal static class SyntaxTokenExtensions
{
    public static bool IsAccessModifier(this in SyntaxToken syntaxToken)
        => syntaxToken.Kind()
        is SyntaxKind.PublicKeyword
        or SyntaxKind.ProtectedKeyword
        or SyntaxKind.PrivateKeyword
        or SyntaxKind.InternalKeyword;
}
