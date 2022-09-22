using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSHack2022.Analyzers;
using System.Collections.Immutable;
using System.Composition;

namespace MSHack2022.Codefixers.Blazor;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class JsInteropFixer : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.JSInvokableMethodsMustBePublic.Id);

    public sealed override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create("Make method public",
                    cancellationToken => MakeMethodPublic(diagnostic, context.Document, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.JSInvokableMethodsMustBePublic.Id),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> MakeMethodPublic(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticTarget = root.FindNode(diagnostic.Location.SourceSpan);
        if (diagnosticTarget is MethodDeclarationSyntax methodDeclaration)
        {
            var oldModifiersWithoutAccess = methodDeclaration.Modifiers
                .Where(st => !st.IsAccessModifier());
            var modifiers = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddRange(oldModifiersWithoutAccess);
            var newMethodDeclaration = methodDeclaration.WithModifiers(modifiers);
            return document.WithSyntaxRoot(root.ReplaceNode(methodDeclaration, newMethodDeclaration));
        }

        return document;
    }
}
