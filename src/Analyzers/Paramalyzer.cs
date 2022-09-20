using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using MSHack2022.Analyzers.Blazor;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class ParamalyzerAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        DiagnosticDescriptors.BadArgumentModifier,
        DiagnosticDescriptors.ExplicitRouteValue,
        DiagnosticDescriptors.ByRefReturnType);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(context =>
        {
            var compilation = context.Compilation;
            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes))
            {
                Debug.Fail("One or more types could not be found.");
                return;
            }

            context.RegisterOperationAction(context =>
            {
                var invocation = (IInvocationOperation)context.Operation;
                var targetMethod = invocation.TargetMethod;
                if (!RouteHandlerHelpers.IsRouteHandlerInvocation(wellKnownTypes!, invocation, targetMethod))
                {
                    return;
                }

                IDelegateCreationOperation? delegateCreation = null;
                foreach (var argument in invocation.Arguments)
                {
                    if (argument.Parameter is not null && argument.Parameter.Ordinal == RouteHandlerHelpers.DelegateParameterOrdinal)
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
                    DetectArgumentModifiers(in context, wellKnownTypes!, invocation, lambda.Symbol);
                    DetectRouteValueUsageLambda(in context, wellKnownTypes!, invocation, lambda.Symbol);
                    DetectRefReturnTypes(in context, wellKnownTypes!, invocation, lambda.Symbol);
                }
                else if (delegateCreation.Target.Kind == OperationKind.MethodReference)
                {
                    var methodReference = (IMethodReferenceOperation)delegateCreation.Target;
                    DetectArgumentModifiers(in context, wellKnownTypes!, invocation, methodReference.Method);
                    DetectRouteValueUsageMethod(in context, wellKnownTypes!, invocation, methodReference.Method);
                    DetectRefReturnTypes(in context, wellKnownTypes!, invocation, methodReference.Method);
                }
            }, OperationKind.Invocation);
        });
    }

    private void DetectArgumentModifiers(in OperationAnalysisContext context,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        foreach (var parameter in methodSymbol.Parameters)
        {
            if (parameter.RefKind != RefKind.None)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.BadArgumentModifier, parameter.Locations[0]));
            }
        }
    }

    private void DetectRefReturnTypes(in OperationAnalysisContext context,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        if (methodSymbol.ReturnType.IsRefLikeType)
        {
            var anyReturnOperations = false;
            foreach (var returnOperation in invocation.Descendants().OfType<IReturnOperation>())
            {
                anyReturnOperations = true;
                if (returnOperation.ReturnedValue is IConversionOperation conversion)
                {
                    if (conversion.Conversion.MethodSymbol is not null &&
                        conversion.Conversion.MethodSymbol.ConstructedFrom.Parameters[0].Type.IsRefLikeType)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ByRefReturnType, returnOperation.Syntax.GetLocation()));
                    }
                }
                else if (returnOperation.ReturnedValue?.Type is not null &&
                    returnOperation.ReturnedValue.Type.IsRefLikeType)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ByRefReturnType, returnOperation.Syntax.GetLocation()));
                }
            }
            // This is likely a method call, e.g. app.MapGet("/", Func);
            // We should grab the location of the method argument for the diagnostic
            if (!anyReturnOperations)
            {
                foreach (var argument in invocation.Arguments)
                {
                    if (argument.Parameter is not null && argument.Parameter.Ordinal == RouteHandlerHelpers.DelegateParameterOrdinal)
                    {
                        var delegateCreation = argument.Descendants().OfType<IDelegateCreationOperation>().FirstOrDefault();
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ByRefReturnType, delegateCreation.Syntax.GetLocation()));
                    }
                }
            }
        }
    }

    private void DetectRouteValueUsageLambda(in OperationAnalysisContext context,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        foreach (var propertyRef in invocation.Descendants().OfType<IPropertyReferenceOperation>())
        {
            if (propertyRef.Member.ContainingType.Name != "RouteValueDictionary")
            {
                continue;
            }
            // TODO: check route pattern as well
            var routeValue = propertyRef.Arguments[0].Value.Syntax.GetFirstToken().Value;

            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExplicitRouteValue,
                ((MemberAccessExpressionSyntax)propertyRef.Instance.Syntax).Name.GetLocation(),
                routeValue));
        }
    }

    private void DetectRouteValueUsageMethod(in OperationAnalysisContext context,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        var methodBody = FindMethodBody(context, invocation, methodSymbol);

        foreach (var propertyRef in methodBody.Descendants().OfType<IPropertyReferenceOperation>())
        {
            if (propertyRef.Member.ContainingType.Name != "RouteValueDictionary")
            {
                continue;
            }
            // TODO: check route pattern as well
            var routeValue = propertyRef.Arguments[0].Value.Syntax.GetFirstToken().Value;

            foreach (var argument in invocation.Arguments)
            {
                if (argument.Parameter is not null && argument.Parameter.Ordinal == RouteHandlerHelpers.DelegateParameterOrdinal)
                {
                    var delegateCreation = argument.Descendants().OfType<IDelegateCreationOperation>().FirstOrDefault();
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ExplicitRouteValue,
                        delegateCreation.Syntax.GetLocation(),
                        routeValue));
                }
            }
        }
    }

    private IBlockOperation? FindMethodBody(OperationAnalysisContext context,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        var syntaxReference = methodSymbol.DeclaringSyntaxReferences.Single();
        var syntaxNode = syntaxReference.GetSyntax(context.CancellationToken);
        var methodOperation = syntaxNode.SyntaxTree == invocation.SemanticModel.SyntaxTree
            ? invocation.SemanticModel.GetOperation(syntaxNode, context.CancellationToken)
            : null;
        if (methodOperation is ILocalFunctionOperation { Body: not null } localFunction)
        {
            return localFunction.Body;
        }
        else if (methodOperation is IMethodBodyOperation methodBody)
        {
            return methodBody.BlockBody ?? methodBody.ExpressionBody;
        }
        return null;
    }
}