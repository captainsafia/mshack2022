using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.Editing;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CodeActions;

namespace MSHack2022.Codefixers;

[ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Factor MapActions into MapGroups")]
public class RouteGroupsCodeRefactoringProvider : CodeRefactoringProvider
{
    public override Task ComputeRefactoringsAsync(CodeRefactoringContext context)
    {
        context.RegisterRefactoring(CodeAction.Create(
                    "Factor to route groups",
                    c => FactorToRouteGroups(context),
                    nameof(FactorToRouteGroups)));

        return Task.CompletedTask;
    }

    public async Task<Document> FactorToRouteGroups(CodeRefactoringContext context)
    {
        var document = context.Document;
        var span = context.Span;
        var cancellationToken = context.CancellationToken;

        var syntaxTree = (await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false))!;
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel is null)
        {
            return document;
        }


        var rootNode = syntaxTree.GetRoot();
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var insertedMapGroup = false;
        SyntaxNode? firstNode = null;
        string? prefix = null;
        foreach (var node in rootNode.DescendantNodes())
        {
            if (node.IsKind(SyntaxKind.InvocationExpression))
            {
                var targetMethodSymbolInfo = semanticModel.GetSymbolInfo(node);
                var targetMethodSymbol = targetMethodSymbolInfo.Symbol;

                if (targetMethodSymbol is IMethodSymbol targetMethod &&
                    targetMethod.Name.StartsWith("Map", StringComparison.Ordinal) &&
                    targetMethod.Parameters.Length == 2)
                {
                    var invocation = (InvocationExpressionSyntax)node;
                    var arguments = invocation.ArgumentList.Arguments;
                    var routePattern = (ArgumentSyntax)arguments[0];
                    var routePatternText = ((LiteralExpressionSyntax)routePattern.Expression).Token.ValueText;
                    var parts = routePatternText.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    prefix = parts[0];
                    if (prefix is null)
                    {
                        continue;
                    }
                    var invocationExpression = (InvocationExpressionSyntax)node;
                    var previousMemberAccessExpression = (MemberAccessExpressionSyntax)invocationExpression.Expression;
                    var innerAccessExpression = (IdentifierNameSyntax)previousMemberAccessExpression.Expression;
                    var newNode = invocationExpression
                        .WithExpression(previousMemberAccessExpression
                            .WithExpression(innerAccessExpression
                                .WithIdentifier(SyntaxFactory.Identifier(prefix))))
                        .WithArgumentList(invocationExpression.ArgumentList.WithArguments(
                            SyntaxFactory.SeparatedList(
                                new[] {
                                    SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(routePatternText.Replace($"/{prefix}", "")))),
                                invocationExpression.ArgumentList.Arguments[1] })));

                    if (!insertedMapGroup)
                    {
                        var nodeGlobalStatement = rootNode.DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Where(
                            node => node.Declaration.Variables.First().Identifier.ValueText == "app").SingleOrDefault();
                        documentEditor.InsertAfter(nodeGlobalStatement, SyntaxFactory.LocalDeclarationStatement(
                            SyntaxFactory.VariableDeclaration(
                                SyntaxFactory.IdentifierName("var"),
                                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(
                                    identifier: SyntaxFactory.Identifier(prefix),
                                    argumentList: null,
                                    initializer: SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                 SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("app"),
                                                SyntaxFactory.IdentifierName("MapGroup")
                                            ),
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList(new[]
                                                {
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal($"/{prefix}")))})))))))));

                        documentEditor
                            .ReplaceNode(node, newNode);


                        insertedMapGroup = true;
                    }
                    else
                    {
                        documentEditor.ReplaceNode(node, newNode);
                    }
                }
            }
        }



        return documentEditor.GetChangedDocument();
    }
}
