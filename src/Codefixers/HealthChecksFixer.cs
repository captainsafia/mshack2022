using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using MSHack2022.Analyzers;
using System.Collections.Immutable;
using System.Composition;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class HealthChecksFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.ShouldHaveHealthChecksCoverage.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create("Add IHealthCheck Coverage",
                    cancellationToken => AddHealthCheckCoverage(diagnostic, context, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.ShouldHaveHealthChecksCoverage.Id),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    private static Task<Solution> AddHealthCheckCoverage(Diagnostic diagnostic, CodeFixContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(context.Document.Project.Solution);
    }
}