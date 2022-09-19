using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace MSHack2022.Analyzers.Blazor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ComponentParameterAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.ComponentsShouldNotWriteToTheirOwnParameters);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
        {
            var compilation = compilationStartAnalysisContext.Compilation;

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes))
            {
                return;
            }

            compilationStartAnalysisContext.RegisterOperationAction(static operationActionAnalysisContext =>
            {
                var compilation = operationActionAnalysisContext.Compilation;

                if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes))
                {
                    return;
                }

                var containingSymbol = operationActionAnalysisContext.ContainingSymbol;
                var containingType = containingSymbol.ContainingType;

                if (!wellKnownTypes.ComponentBase.IsBaseTypeOf(containingType))
                {
                    return;
                }

                if (operationActionAnalysisContext.Operation is IAssignmentOperation assignment &&
                    assignment.Target is IPropertyReferenceOperation propertyReference &&
                    IsParameterProperty(propertyReference.Property, wellKnownTypes) &&
                    !ShouldContainingSymbolPermitParameterWriting(containingSymbol, wellKnownTypes))
                {
                    operationActionAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ComponentsShouldNotWriteToTheirOwnParameters,
                        assignment.Syntax.GetLocation(),
                        propertyReference.Property.Name));
                }
            }, OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment);
        });
    }

    private static bool ShouldContainingSymbolPermitParameterWriting(ISymbol containingSymbol, WellKnownTypes wellKnownTypes)
    {
        if (containingSymbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (methodSymbol.MethodKind == MethodKind.Constructor)
        {
            return true;
        }

        if (wellKnownTypes.SetParametersAsync.IsOverriddenBy(methodSymbol))
        {
            return true;
        }

        return false;
    }

    private static bool IsParameterProperty(IPropertySymbol property, WellKnownTypes wellKnownTypes)
        => property.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(
            wellKnownTypes.ParameterAttribute,
            a.AttributeClass));
}
