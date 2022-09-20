using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class WithNameAnalyzer : DiagnosticAnalyzer
{
    private const int DelegateParameterOrdinal = 2;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.WithName);

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

            if (delegateCreation.Target.Kind == OperationKind.AnonymousFunction)
            {
                var lambda = (IAnonymousFunctionOperation)delegateCreation.Target;
                SuggestApiName(in context, targetMethod, wellKnownTypes, invocation, lambda.Symbol);

            }
        }, OperationKind.Invocation);
    }

    private static void SuggestApiName(
        in OperationAnalysisContext context,
        IMethodSymbol mapMethod,
        WellKnownTypes wellKnownTypes,
        IInvocationOperation invocation,
        IMethodSymbol methodSymbol)
    {
        // Exclude if there is already a WithName() extension call
        var currentAncestor = invocation.Syntax.Parent;
        while (true)
        {
            if (currentAncestor is MemberAccessExpressionSyntax
                || currentAncestor is ExpressionStatementSyntax
                || currentAncestor is null)
            {
                break;
            }
            currentAncestor = currentAncestor.Parent;
        }

        if (currentAncestor is MemberAccessExpressionSyntax mae && mae.Name.Identifier.Text == "WithName")
        {
            return;
        }

        // Exclude if there is already an [EndpointName] attribute on the delegate
        if (methodSymbol.HasAttribute(wellKnownTypes.EndpointNameAttribute))
        {
            return;
        }

        var verb = GetVerbFromMapMethodName(mapMethod.Name);
        if (verb is null)
        {
            return;
        }

        // Get suggested resource/noun name from route pattern

        var suggestedApiName = verb + "TemporaryApiName";

        var location = invocation.Syntax.GetLocation();

        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("SuggestedApiName", suggestedApiName);

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.WithName,
            location,
            properties.ToImmutable(),
            suggestedApiName));

        //foreach (var parameter in methodSymbol.Parameters)
        //{
        //    var modelBindingAttribute = parameter.GetAttributes(wellKnownTypes.IBinderTypeProviderMetadata).FirstOrDefault() ??
        //        parameter.GetAttributes(wellKnownTypes.BindAttribute).FirstOrDefault();

        //    if (modelBindingAttribute is not null)
        //    {
        //        var location = invocation.Syntax.GetLocation();

        //        context.ReportDiagnostic(Diagnostic.Create(
        //            DiagnosticDescriptors.WithName,
        //            location,
        //            suggestedMethodName));
        //    }
        //}
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

    private static string? GetVerbFromMapMethodName(string mapMethodName)
    {
        if (mapMethodName.Length == 3) return null;

        var suffix = mapMethodName.Substring(3);

        return suffix switch
        {
            "Get" => suffix,
            "Post" => "Create",
            "Put" => "Update",
            "Delete" => suffix,
            _ => null
        };
    }
}