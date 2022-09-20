using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Tags;
using MSHack2022.Analyzers;
using System.Collections.Immutable;
using System.Composition;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class MoveMiddlewareToClassFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(DiagnosticDescriptors.MoveMiddlewareToClass.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            // TODO: Support multiple suggested class locations, e.g. nested, same file, new file
            context.RegisterCodeFix(
                CodeAction.Create("Convert to middleware class",
                    cancellationToken => MoveToMiddlewareClass(diagnostic, context.Document, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.MoveMiddlewareToClass.Id),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> MoveToMiddlewareClass(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticTarget = root.FindNode(diagnostic.Location.SourceSpan);
        
        if (diagnosticTarget is InvocationExpressionSyntax invocationExpression)
        {
            var suggestedApiName = diagnostic.Properties["SuggestedApiName"] ?? "MyApi";

            var withNameMemberAccess = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression, invocationExpression, SyntaxFactory.IdentifierName("WithName"));
            var withNameInvocation = SyntaxFactory.InvocationExpression(
                withNameMemberAccess, SyntaxFactory.ArgumentList(
                    SyntaxFactory.SeparatedList(new[] {
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(suggestedApiName)))
                    })));

            return document.WithSyntaxRoot(root.ReplaceNode(invocationExpression, withNameInvocation));
        }

        return document;
    }
}