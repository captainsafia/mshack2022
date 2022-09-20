using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;

namespace MSHack2022.Analyzers.Blazor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EventCallbackAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.EventCallbackCapturingForLoopIteratorVariable);

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

            compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
            {
                if (operationAnalysisContext.Operation is ILocalReferenceOperation localReferenceOperation &&
                    IsLocalDefinedByForStatement(localReferenceOperation.Local, operationAnalysisContext.CancellationToken) &&
                    IsLocalReferenceCapturedInEventCallback(localReferenceOperation, wellKnownTypes))
                {
                    operationAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticDescriptors.EventCallbackCapturingForLoopIteratorVariable,
                        localReferenceOperation.Syntax.GetLocation(),
                        localReferenceOperation.Local.Name));
                }
            }, OperationKind.LocalReference);
        });
    }

    private static bool IsLocalDefinedByForStatement(
        ILocalSymbol? localSymbol,
        CancellationToken cancellationToken)
        => localSymbol?.DeclaringSyntaxReferences.SingleOrDefault() is { } syntaxReference
        && syntaxReference.GetSyntax(cancellationToken) is { } declaratorSyntax
        && declaratorSyntax.FirstAncestorOrSelf<VariableDeclarationSyntax>() is { } declarationSyntax
        && declarationSyntax.FirstAncestorOrSelf<ForStatementSyntax>() is { } forStatementSyntax
        && forStatementSyntax.Declaration == declarationSyntax;

    private static bool IsLocalReferenceCapturedInEventCallback(
        ILocalReferenceOperation localReferenceOperation,
        WellKnownTypes wellKnownTypes)
        => localReferenceOperation.FirstAncestorOrSelf<IAnonymousFunctionOperation>() is { } anonymousFunctionOperation
        && anonymousFunctionOperation.FirstAncestorOrSelf<IArgumentOperation>() is { } argumentOperation
        && argumentOperation.FirstAncestorOrSelf<IInvocationOperation>() is { } invocationOperation
        && SymbolEqualityComparer.Default.Equals(
            wellKnownTypes.EventCallbackFactory,
            invocationOperation.TargetMethod.ContainingSymbol);
}
