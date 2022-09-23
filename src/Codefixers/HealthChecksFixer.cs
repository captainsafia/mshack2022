using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using MSHack2022.Analyzers;
using MSHack2022.Codefixers.Extensions;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;

namespace MSHack2022.Codefixers;

[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
public class HealthChecksFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticDescriptors.ShouldHaveHealthChecksCoverage.Id);

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            var methodName = diagnostic.Properties["methodName"];
            context.RegisterCodeFix(
                CodeAction.Create($"Add health check coverage for {methodName}.",
                    cancellationToken => AddHealthCheckCoverage(diagnostic, context, cancellationToken),
                    equivalenceKey: DiagnosticDescriptors.ShouldHaveHealthChecksCoverage.Id),
                diagnostic);
        }

        return Task.CompletedTask;
    }

    private static async Task<Solution> AddHealthCheckCoverage(Diagnostic diagnostic, CodeFixContext context, CancellationToken cancellationToken)
    {
        //Debugger.Launch();

        var solution = context.Document.Project.Solution;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null)
        {
            return solution;
        }

        var compilation = semanticModel.Compilation;
        if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes, out var failedType))
        {
            Debug.Fail($"{failedType} could not be found.");
            return solution;
        }

        var implementedHealthChecks = await SymbolFinder.FindImplementationsAsync(wellKnownTypes.IHealthCheck, solution);
        var aCheck = implementedHealthChecks.FirstOrDefault()?.GetSyntaxNodes().FirstOrDefault();
        if (aCheck == null)
        {
            return solution;
        }

        var aCheckTree = aCheck.GetLocation().SourceTree;
        if (aCheckTree == null)
        {
            return solution;
        }

        var aCheckDocument = solution.GetDocument(aCheckTree);
        if (aCheckDocument == null)
        {
            return solution;
        }

        var healthCheckClassName = diagnostic.Properties["healthCheckName"];

        var aCheckRoot = await aCheckTree.GetRootAsync();
        var aCheckClassDeclaration = aCheckRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

        static string GetNamespaceFrom(SyntaxNode s) => s.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax.Name.ToString(),
            null => string.Empty,
            _ => GetNamespaceFrom(s.Parent)
        };

        var healthCheckNamespace = GetNamespaceFrom(aCheckClassDeclaration);
        if (string.IsNullOrEmpty(healthCheckNamespace))
        {
            return solution;
        }

        var newHealthCheckClassText = _parsingTreeTemplate
            .Replace("<NAMESPACE>", healthCheckNamespace).Replace("<HEALTHCHECK_CLASS_NAME>", healthCheckClassName);

        var filePath = Path.Combine(Directory.GetParent(aCheckDocument.FilePath).FullName, $"{healthCheckClassName}.cs");

        var newHealthCheckDocument = aCheckDocument.Project.AddDocument($"{healthCheckClassName}.cs", newHealthCheckClassText, filePath: filePath);

        return newHealthCheckDocument.Project.Solution;
    }

    private const string _parsingTreeTemplate = @"
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace <NAMESPACE>
{
    public class <HEALTHCHECK_CLASS_NAME> : IHealthCheck
    {
        public <HEALTHCHECK_CLASS_NAME>()
        {
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
";

}