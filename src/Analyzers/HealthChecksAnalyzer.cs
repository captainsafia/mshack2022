using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers;

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
        var httpMethodSymbols = new ConcurrentDictionary<string, IMethodSymbol>();
        //var checkHealthMethodSymbols = new ConcurrentDictionary<string, IMethodSymbol>();
        var healthCheckTypeSymbols = new ConcurrentDictionary<string, ITypeSymbol>();
        var httpMethodToHealtCheckMap = new ConcurrentDictionary<string, string>();

        startContext.RegisterSymbolAction(symbolContext => AnalyzeHttpMethodSymbols(symbolContext, httpMethodSymbols), SymbolKind.Method);
        //startContext.RegisterSymbolAction(symbolContext => AnalyzeCheckHealthMethodSymbols(symbolContext, checkHealthMethodSymbols), SymbolKind.Method);
        startContext.RegisterSymbolAction(symbolContext => AnalyzeHealthCheckTypeSymbols(symbolContext, healthCheckTypeSymbols), SymbolKind.NamedType);

        startContext.RegisterCompilationEndAction(compilationContext => AnalyzeCompilationEnd(compilationContext, httpMethodSymbols, healthCheckTypeSymbols));
    }

    
    private void AnalyzeHttpMethodSymbols(SymbolAnalysisContext symbolContext, ConcurrentDictionary<string, IMethodSymbol> httpMethodSymbols)
    {
        if (!WellKnownTypes.TryCreate(symbolContext.Compilation, out var wellKnownTypes, out var failedType))
        {
            Debug.Fail($"{failedType} could not be found.");
            return;
        }

        // Check whether this is an HTTP method
        var methodSymbol = (IMethodSymbol)symbolContext.Symbol;
        var hasHttpAttribute = methodSymbol.HasAttribute(wellKnownTypes.HttpMethodAttribute);
        if (hasHttpAttribute)
        {
            httpMethodSymbols.TryAdd(FullyQualifyHttpMethodName(methodSymbol), methodSymbol);
        }
    }

    private string FullyQualifyHttpMethodName(IMethodSymbol httpMethodSymbol)
    {
        // TODO handle parameters
        return $"{httpMethodSymbol.Name}{httpMethodSymbol.ContainingType.Name.Replace("Controller", "")}";
    }

    private void AnalyzeHealthCheckTypeSymbols(SymbolAnalysisContext symbolContext, ConcurrentDictionary<string, ITypeSymbol> healthCheckTypeSymbols)
    {
        if (!WellKnownTypes.TryCreate(symbolContext.Compilation, out var wellKnownTypes, out var failedType))
        {
            Debug.Fail($"{failedType} could not be found.");
            return;
        }

        var typeSymbol = (ITypeSymbol)symbolContext.Symbol;
        if (typeSymbol.Implements(wellKnownTypes.IHealthCheck))
        {
            healthCheckTypeSymbols.TryAdd(typeSymbol.Name, typeSymbol);
        }
    }


    private void AnalyzeCompilationEnd(CompilationAnalysisContext endContext, ConcurrentDictionary<string, IMethodSymbol> httpMethodSymbols, ConcurrentDictionary<string, ITypeSymbol> healthCheckTypeSymbols)
    {
        foreach (var httpMethodSymbol in httpMethodSymbols)
        {
            if (!healthCheckTypeSymbols.ContainsKey($"{httpMethodSymbol.Key}HealthCheck"))
            {
                var httpMethodDeclarations = httpMethodSymbol.Value.GetSyntaxNodes();
                foreach (var httpMethodDeclaration in httpMethodDeclarations)
                {
                    var diagnostic = Diagnostic.Create(DiagnosticDescriptors.ShouldHaveHealthChecksCoverage, httpMethodDeclaration.GetLocation());
                    endContext.ReportDiagnostic(diagnostic);
                }

                // We might as well go deeper in the check and trying to infer on a best-effort basis
                // whether there is a common codepath between the Controller and the HealthCheck.
                // This can either be in the form of a processor invocation, or http-path (less reliable source).
            }
        }

        // TODO AddCheck for existing HC
    }
}