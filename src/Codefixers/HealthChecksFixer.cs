using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class HealthChecksFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(
        "MH015");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        Debugger.Launch();

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null)
        {
            return;
        }

        var compilation = semanticModel.Compilation;
        if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes, out var failedType))
        {
            Debug.Fail($"{failedType} could not be found.");
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(
                CodeAction.Create("Add IHealthCheck Coverage",
                    cancellationToken => AddHealthCheckCoverage(diagnostic, context, semanticModel, wellKnownTypes, cancellationToken),
                    equivalenceKey: "MH015"),
                diagnostic);
        }

        return;
    }

    private static Task<Solution> AddHealthCheckCoverage(Diagnostic diagnostic, CodeFixContext context, SemanticModel semanticModel, WellKnownTypes wellKnownTypes, CancellationToken cancellationToken)
    {
        var implementedHealthChecks = SymbolFinder.FindImplementationsAsync(wellKnownTypes.IHealthCheck, context.Document.Project.Solution);

        Debugger.Launch();

        return Task.FromResult(context.Document.Project.Solution);
    }
}