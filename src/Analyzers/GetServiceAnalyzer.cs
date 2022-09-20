using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class GetServiceAnalyzer : DiagnosticAnalyzer
{
    private const int DelegateParameterOrdinal = 2;

     public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.GetService);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();


        context.RegisterCompilationStartAction(static context =>
        {
            var compilation = context.Compilation;
            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes, out var failedType))
            {
                Debug.Fail($"{failedType} could not be found.");
                return;
            }

            context.RegisterOperationAction(context =>
            {
                var invocation = (IInvocationOperation)context.Operation;
                var targetMethod = invocation.TargetMethod;

                if (!IsRouteHandlerInvocation(wellKnownTypes, invocation, targetMethod))
                {
                    return;
                }

                IDelegateCreationOperation? delegateCreation = null;
                foreach (var argument in invocation.Arguments)
                {
                    if (argument.Parameter?.Ordinal == DelegateParameterOrdinal)
                    {
                        delegateCreation = argument.Descendants().OfType<IDelegateCreationOperation>().FirstOrDefault();
                        break;
                    }
                }

                if (delegateCreation is null)
                {
                    return;
                }

                IBlockOperation? methodBody = null;

                if (delegateCreation.Target.Kind == OperationKind.AnonymousFunction)
                {
                    var lambda = (IAnonymousFunctionOperation)delegateCreation.Target;
                    methodBody = lambda.Body;
                }
                else if (delegateCreation.Target.Kind == OperationKind.MethodReference)
                {
                    var methodReference = (IMethodReferenceOperation)delegateCreation.Target;

                    if (!methodReference.Method.DeclaringSyntaxReferences.IsEmpty)
                    {
                        var syntaxReference = methodReference.Method.DeclaringSyntaxReferences.Single();
                        var syntaxNode = syntaxReference.GetSyntax(context.CancellationToken);
                        var methodOperation = syntaxNode.SyntaxTree == invocation.SemanticModel?.SyntaxTree
                            ? invocation.SemanticModel.GetOperation(syntaxNode, context.CancellationToken)
                            : null;

                        if (methodOperation is ILocalFunctionOperation { Body: not null } localFunction)
                        {
                            methodBody = localFunction.Body;
                        }
                        else if (methodOperation is IMethodBodyOperation methodBodyOperation)
                        {
                            methodBody = methodBodyOperation.BlockBody;
                        }
                    }
                }


                if (HasGetServiceCall(wellKnownTypes, methodBody))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.GetService, delegateCreation.Syntax.GetLocation()));
                }
            }, OperationKind.Invocation);
        });
    }

    private static bool HasGetServiceCall(WellKnownTypes wellKnownTypes, IBlockOperation? methodBody)
    {
        if (methodBody is null)
        {
            return false;
        }
        
        foreach (var invocationOperation in methodBody.Descendants().OfType<IInvocationOperation>())
        {
            if (invocationOperation.TargetMethod.ContainingType.Equals(wellKnownTypes.IServiceProvider, SymbolEqualityComparer.Default))
            {
                if (invocationOperation.TargetMethod.Name == nameof(IServiceProvider.GetService))
                {
                    return true;
                }
            }
            else if (invocationOperation.TargetMethod.ContainingType.Equals(wellKnownTypes.ServiceProviderExtensions, SymbolEqualityComparer.Default))
            {
                if (invocationOperation.TargetMethod.Name == "GetService" || invocationOperation.TargetMethod.Name == "GetRequiredService")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsRouteHandlerInvocation(
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol targetMethod)
    {
        return targetMethod.Name.StartsWith("Map", StringComparison.Ordinal) &&
            SymbolEqualityComparer.Default.Equals(wellKnownTypes.EndpointRouteBuilderExtensions, targetMethod.ContainingType) &&
            invocation.Arguments.Length == 3 &&
            targetMethod.Parameters.Length == 3 &&
            SymbolEqualityComparer.Default.Equals(wellKnownTypes.Delegate, targetMethod.Parameters[DelegateParameterOrdinal].Type);
    }
}