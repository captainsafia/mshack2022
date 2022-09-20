using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class MoveMiddlewareToClassAnalyzer : DiagnosticAnalyzer
{
    private const int MiddlewareParameterOrdinal = 1;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.MoveMiddlewareToClass);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(static context =>
        {
            var compilation = context.Compilation;

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes) || wellKnownTypes is null)
            {
                Debug.Fail("One or more types could not be found. This usually means you are bad at spelling C# type names.");
                return;
            }

            var invocation = (IInvocationOperation)context.Operation;
            var targetMethod = invocation.TargetMethod;
            if (!IsInlineMiddlewareInvocation(wellKnownTypes, invocation, targetMethod))
            {
                return;
            }

            IDelegateCreationOperation? delegateCreation = null;
            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter?.Ordinal == MiddlewareParameterOrdinal)
                {
                    delegateCreation = argument.Descendants().OfType<IDelegateCreationOperation>().FirstOrDefault();
                    break;
                }
            }

            if (delegateCreation is null)
            {
                return;
            }

            if (delegateCreation.Target.Kind == OperationKind.AnonymousFunction)
            {
                var lambda = (IAnonymousFunctionOperation)delegateCreation.Target;
                SuggestMoveToClass(in context, targetMethod, wellKnownTypes, invocation, lambda.Symbol);
            }
        }, OperationKind.Invocation);
    }

    private static void SuggestMoveToClass(
        in OperationAnalysisContext context,
        IMethodSymbol middlewareMethod,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        var location = invocation.Syntax.GetLocation();

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.MoveMiddlewareToClass,
            location));
    }

    private static bool IsInlineMiddlewareInvocation(
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol targetMethod)
    {
        return string.Equals(targetMethod.Name, "Use", StringComparison.Ordinal) &&
            SymbolEqualityComparer.Default.Equals(wellKnownTypes.UseExtensions, targetMethod.ContainingType) &&
            invocation.Arguments.Length == 2 &&
            targetMethod.Parameters.Length == 2 &&
            SymbolEqualityComparer.Default.Equals(wellKnownTypes.Func_HttpContext_RequestDelegate_Task, targetMethod.Parameters[MiddlewareParameterOrdinal].Type);
    }
}