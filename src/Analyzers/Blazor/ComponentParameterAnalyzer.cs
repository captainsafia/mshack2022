using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers.Blazor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ComponentParameterAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.ComponentsShouldNotWriteToTheirOwnParameters,
        DiagnosticDescriptors.MissingParameterAttribute);

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

            var attributesRequiringParameterAttribute = ImmutableHashSet.Create(
                SymbolEqualityComparer.Default,
                wellKnownTypes.SupplyParameterFromQueryAttribute,
                wellKnownTypes.EditorRequiredAttribute);

            compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
            {
                var containingSymbol = operationAnalysisContext.ContainingSymbol;
                var containingType = containingSymbol.ContainingType;

                if (!wellKnownTypes.ComponentBase.IsBaseTypeOf(containingType))
                {
                    return;
                }

                if (operationAnalysisContext.Operation is IAssignmentOperation assignment &&
                    assignment.Target is IPropertyReferenceOperation propertyReference &&
                    IsParameterProperty(propertyReference.Property, wellKnownTypes) &&
                    !ShouldContainingSymbolPermitParameterWriting(containingSymbol, wellKnownTypes))
                {
                    operationAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.ComponentsShouldNotWriteToTheirOwnParameters,
                        assignment.Syntax.GetLocation(),
                        propertyReference.Property.Name));
                }
            }, OperationKind.SimpleAssignment, OperationKind.CompoundAssignment, OperationKind.CoalesceAssignment);

            compilationStartAnalysisContext.RegisterSymbolAction(symbolAnalysisContext =>
            {
                if (symbolAnalysisContext.Symbol is IPropertySymbol propertySymbol &&
                    !propertySymbol.HasAttribute(wellKnownTypes.ParameterAttribute))
                {
                    foreach (var attribute in propertySymbol.GetAttributes())
                    {
                        if (attribute.AttributeClass is { } attributeClass &&
                            attributesRequiringParameterAttribute.Contains(attributeClass))
                        {
                            var cancellationToken = symbolAnalysisContext.CancellationToken;
                            var attributeLocation = attribute.ApplicationSyntaxReference?.GetSyntax(cancellationToken)?.GetLocation();
                            var location = attributeLocation ?? propertySymbol.Locations.FirstOrDefault();

                            symbolAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                                DiagnosticDescriptors.MissingParameterAttribute,
                                location,
                                attributeClass.Name));
                        }
                    }
                }
            }, SymbolKind.Property);
        });
    }

    private static bool ShouldContainingSymbolPermitParameterWriting(ISymbol containingSymbol, WellKnownTypes wellKnownTypes)
        => containingSymbol is IMethodSymbol methodSymbol
        && (methodSymbol.MethodKind == MethodKind.Constructor || wellKnownTypes.SetParametersAsync.IsOverriddenBy(methodSymbol));

    private static bool IsParameterProperty(IPropertySymbol property, WellKnownTypes wellKnownTypes)
        => property.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(
            wellKnownTypes.ParameterAttribute,
            a.AttributeClass));
}
