using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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

        context.RegisterSymbolAction(static context =>
        {
            var compilation = context.Compilation;

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes) || wellKnownTypes is null)
            {
                Debug.Fail("One or more types could not be found. This usually means you are bad at spelling C# type names.");
                return;
            }

            // Check whether this is an HTTP method
            var methodSymbol = (IMethodSymbol)context.Symbol;
            methodSymbol.HasAttribute(wellKnownTypes.HttpMethodAttribute);

            // Try to infer the underling processor or HTTP path
            // There might be different pathways to the health check
            //
            // 1) If processor (no HTTP call), the corresponding HCs, if existing,
            //    is the parent of the least common ancestor (LCA)
            // 
            // 2) If HTTP based (i.e. auto generated rest client calling the controller),
            //    there is no reliable rel at the source and semantic level we can leverage
            
            // We'll focus on the first strategy

        }, SymbolKind.Method);
    }
}