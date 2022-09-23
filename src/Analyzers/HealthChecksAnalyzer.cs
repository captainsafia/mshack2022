using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using MSHack2022.Analyzers.Extensions;
using MSHack2022.Analyzers.Visitors;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers;

// Reference stateful analyzer:
// https://github.com/dotnet/roslyn-sdk/blob/main/samples/CSharp/Analyzers/Analyzers.Implementation/StatefulAnalyzers/CompilationStartedAnalyzer.cs

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class HealthChecksAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.ShouldHaveHealthChecksCoverage);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(AnalyzeCompilationStart);
    }

    private void AnalyzeCompilationStart(CompilationStartAnalysisContext startContext)
    {
        var healthCheckTypeSymbols = new ConcurrentDictionary<string, ITypeSymbol>();
        var httpMethodToHealtCheckMap = new ConcurrentDictionary<string, string>();

        if (!WellKnownTypes.TryCreate(startContext.Compilation, out var wellKnownTypes, out var failedType))
        {
            Debug.Fail($"{failedType} could not be found.");
            return;
        }

        var healthCheckTypeSymbols_ = NamedTypeSymbolFinder.GetAllSymbols(startContext.Compilation, predicate: t => t.Implements(wellKnownTypes.IHealthCheck));
        foreach (var hcTypeSymbol in healthCheckTypeSymbols_)
        {
            healthCheckTypeSymbols.TryAdd(hcTypeSymbol.Name, hcTypeSymbol);
        }

        startContext.RegisterSymbolAction(symbolContext => AnalyzeHttpMethodSymbols(symbolContext, healthCheckTypeSymbols), SymbolKind.Method);
    }

    private void AnalyzeHttpMethodSymbols(SymbolAnalysisContext symbolContext, ConcurrentDictionary<string, ITypeSymbol> healthCheckTypeSymbols)
    {
        if (!WellKnownTypes.TryCreate(symbolContext.Compilation, out var wellKnownTypes, out var failedType))
        {
            Debug.Fail($"{failedType} could not be found.");
            return;
        }

        // Check whether this is an HTTP method
        var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
        var hasHttpAttribute = methodSymbol.HasAttribute(wellKnownTypes.HttpMethodAttribute);
        var healthCheckName = $"{FullyQualifyHttpMethodName(methodSymbol)}HealthCheck";

        if (hasHttpAttribute && !healthCheckTypeSymbols.ContainsKey($"{FullyQualifyHttpMethodName(methodSymbol)}HealthCheck"))
        {
            var httpMethodDeclarations = methodSymbol.GetSyntaxNodes();
            foreach (var httpMethodDeclaration in httpMethodDeclarations)
            {
                var diagnosticProperties = new Dictionary<string, string?>()
                {
                    { "healthCheckName", healthCheckName },
                    { "methodName", methodSymbol.Name },
                }.ToImmutableDictionary();

                var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ShouldHaveHealthChecksCoverage, httpMethodDeclaration.GetLocation(), diagnosticProperties);
                symbolContext.ReportDiagnostic(diagnostic);

                // We might as well go deeper in the check and trying to infer on a best-effort basis
                // whether there is a common codepath between the Controller and the HealthCheck.
                // This can either be in the form of a processor invocation, or http-path (less reliable source).

                // TODO AddCheck for existing HC
            }
        }
    }

    private string FullyQualifyHttpMethodName(IMethodSymbol httpMethodSymbol)
    {
        return $"{httpMethodSymbol.Name}{httpMethodSymbol.ContainingType.Name.Replace("Controller", "")}";
    }

    
}