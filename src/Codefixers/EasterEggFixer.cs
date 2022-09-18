using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using MSHack2022.Analyzers;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class EasterEggFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.MeaningOfLife.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create("Update variable name to 'meaningOfLife'",
                    cancellationToken => UpdateVariableNameToMeaningOfLife(diagnostic, context.Document, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.MeaningOfLife.Id),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    private static async Task<Document> UpdateVariableNameToMeaningOfLife(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticTarget = root.FindNode(diagnostic.Location.SourceSpan);
        if (diagnosticTarget is LocalDeclarationStatementSyntax localDeclarationStatement)
        {
            var declaration = localDeclarationStatement.Declaration;
            if (declaration.Variables.Count() > 1)
            {
                return document;
            }
            var variableDeclaration = declaration.Variables.Single();
            var renamedDeclaration = variableDeclaration.WithIdentifier(SyntaxFactory.Identifier("meaningOfLife"));
            return document.WithSyntaxRoot(root.ReplaceNode(variableDeclaration, renamedDeclaration));
        }

        return document;
    }
}