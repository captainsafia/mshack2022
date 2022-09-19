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
        context.RegisterOperationAction(static operationActionAnalysisContext =>
        {
            var compilation = operationActionAnalysisContext.Compilation;

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes))
            {
                // Not in a Blazor project.
                return;
            }

            var containingSymbol = operationActionAnalysisContext.ContainingSymbol;
            var containingType = containingSymbol.ContainingType;

            if (!SymbolEqualityComparer.Default.Equals(wellKnownTypes.ComponentBase, containingType?.BaseType))
            {
                // Not a Razor component extending "ComponentBase".
                // TODO: Handle the case where the "ComponentBase" type is higher up in the type heirarchy.
                return;
            }

            var operation = operationActionAnalysisContext.Operation;

            if (!ShouldContainingSymbolPermitParameterWriting(containingSymbol, wellKnownTypes) &&
                operation is IAssignmentOperation assignment &&
                assignment.Target is IPropertyReferenceOperation propertyReference &&
                IsParameterProperty(propertyReference.Property, wellKnownTypes))
            {
                operationActionAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.ComponentsShouldNotWriteToTheirOwnParameters,
                    propertyReference.Syntax.GetLocation(),
                    propertyReference.Property.Name));
            }
        }, OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment);
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

        if (SymbolEqualityComparer.Default.Equals(wellKnownTypes.SetParametersAsync, methodSymbol.OverriddenMethod))
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
