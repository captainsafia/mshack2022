using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis;
using MSHack2022.Analyzers;
using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class RouteGroupFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.RecommendUsingRouteGroups.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            //context.RegisterCodeFix(
            //    CodeAction.Create("Use route groups",
            //        cancellationToken => UpdateCodeToUseRouteGroups(diagnostic, context.Document, cancellationToken),
            //        equivalenceKey: DiagnosticDescriptors.RecommendUsingRouteGroups.Id),
            //    diagnostic);
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> UpdateCodeToUseRouteGroups(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticTarget = root.FindNode(diagnostic.Location.SourceSpan);
        if (diagnosticTarget is InvocationExpressionSyntax invocationExpression)
        {
            var previousMemberAccessExpression = (MemberAccessExpressionSyntax)invocationExpression.Expression;
            var innerAccessExpression = (IdentifierNameSyntax)previousMemberAccessExpression.Expression;
            var prefixName = "foo";
            var routePatternWithoutPrefix = "bar";
            var updatedInvocationExpression = invocationExpression
                .WithExpression(previousMemberAccessExpression
                    .WithExpression(innerAccessExpression
                        .WithIdentifier(SyntaxFactory.Identifier(prefixName))))
                .WithArgumentList(invocationExpression.ArgumentList.WithArguments(
                    SyntaxFactory.SeparatedList(
                        new[] {
                            SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(routePatternWithoutPrefix))),
                        invocationExpression.ArgumentList.Arguments[1] })));
            return document.WithSyntaxRoot(root.ReplaceNode(invocationExpression, updatedInvocationExpression));
        }

        return document;
    }
}