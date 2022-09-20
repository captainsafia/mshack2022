using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSHack2022.Analyzers;
using System.Collections.Immutable;
using System.Composition;

namespace MSHack2022.Codefixers
{
    [ExportCodeFixProvider(LanguageNames.CSharp), Shared]
    public class ParamFixer : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticDescriptors.BadArgumentModifier.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                context.RegisterCodeFix(CodeAction.Create("Remove invalid parameter modifier",
                    cancellationToken => RemoveModifier(diagnostic, context.Document, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.BadArgumentModifier.Id),
                    diagnostic);
            }

            return Task.CompletedTask;
        }

        private async Task<Document> RemoveModifier(Diagnostic diagnostic, Document document, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root == null)
            {
                return document;
            }

            var diagnosticTarget = root.FindNode(diagnostic.Location.SourceSpan);
            if (diagnosticTarget is ParameterSyntax parameter)
            {
                return document.WithSyntaxRoot(root.ReplaceNode(parameter, parameter.WithModifiers(new SyntaxTokenList())));
            }

            return document;
        }
    }
}
