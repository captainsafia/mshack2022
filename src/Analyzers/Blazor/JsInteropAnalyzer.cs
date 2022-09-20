using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers.Blazor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class JsInteropAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.ShouldNotPerformJsInteropInOnInitializedAsync);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
        {
            var compilation = compilationStartAnalysisContext.Compilation;

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes, out var failedType))
            {
                Debug.Fail($"{failedType} could not be found.");
                return;
            }

            compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
            {
                var containingSymbol = operationAnalysisContext.ContainingSymbol;
                var containingType = containingSymbol.ContainingType;

                if (!wellKnownTypes.ComponentBase.IsBaseTypeOf(containingType))
                {
                    return;
                }

                if (containingSymbol is IMethodSymbol containingMethod &&
                    operationAnalysisContext.Operation is IInvocationOperation invocation &&
                    wellKnownTypes.OnInitializedAsync.IsOverriddenBy(containingMethod) &&
                    SymbolEqualityComparer.Default.Equals(wellKnownTypes.IJSRuntime, invocation.GetInstanceType()))
                {
                    operationAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ShouldNotPerformJsInteropInOnInitializedAsync,
                        invocation.Syntax.GetLocation()));
                }

            }, OperationKind.Invocation);
        });
    }
}
