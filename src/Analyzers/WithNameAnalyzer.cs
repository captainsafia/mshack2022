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

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes, out var failedType) || wellKnownTypes is null)
            {
                Debug.Fail($"{failedType} could not be found.");
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

        string resourceName = GetResourceNameFromRoutePattern(verb, invocation, out var by);

        var suggestedApiName = verb + resourceName;

        if (by is not null)
        {
            suggestedApiName += "By" + Capitalize(by);
        }

        var location = invocation.Syntax.GetLocation();

        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("SuggestedApiName", suggestedApiName);

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticDescriptors.WithName,
            location,
            properties.ToImmutable(),
            suggestedApiName));
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

    private static string GetResourceNameFromRoutePattern(string verb, IInvocationOperation invocation, out string? by)
    {
        var resourceName = "";
        by = null;

        // Get suggested resource/noun name from route pattern if it's a literal
        if (invocation.Arguments[1].Syntax is ArgumentSyntax argSyntax
            && argSyntax.Expression is LiteralExpressionSyntax routePatternLiteral)
        {
            // TODO: Do proper route pattern parsing here
            var routePatternText = routePatternLiteral.Token.ValueText;
            var parts = routePatternText.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            string? lastPartWithoutParameter = null;
            var parameters = new List<string>();

            for (int i = parts.Length - 1; i >= 0; i--)
            {
                var part = parts[i];

                if (!(part.StartsWith("{") && part.EndsWith("}")))
                {
                    lastPartWithoutParameter ??= part;
                }
                else
                {
                    parameters.Add(part.Substring(1, part.Length - 2));
                }
            }

            if (parameters.Count > 0)
            {
                by = parameters[parameters.Count - 1];
            }

            if (lastPartWithoutParameter is not null)
            {
                resourceName = Capitalize(lastPartWithoutParameter);

                // De-pluralize for Create/Update
                // HACK: Should use something like Humanizer for this
                if ((verb == "Create" || verb == "Update") && resourceName.EndsWith("s"))
                {
                    resourceName = resourceName.Substring(0, resourceName.Length - 1);
                }
            }
        }

        return resourceName;
    }

    private static string Capitalize(string word)
    {
        if (word.Length > 1)
        {
            return Char.ToUpper(word[0]) + word.Substring(1);
        }
        else if (word.Length == 1)
        {
            return Char.ToUpper(word[0]).ToString();
        }

        return word;
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