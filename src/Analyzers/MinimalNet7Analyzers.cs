using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using MSHack2022.Analyzers.Blazor;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class MinimalNet7Analyzers : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.UseDotnetUserJwtsTool,
        DiagnosticDescriptors.RecommendUsingRouteGroups);

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

            context.RegisterSemanticModelAction(context =>
            {
                var rootNode = context.SemanticModel.SyntaxTree.GetRoot();
                var encounteredRoutes = new Dictionary<string, List<Location>>();
                foreach (var node in rootNode.DescendantNodes())
                {
                    if (node.IsKind(SyntaxKind.InvocationExpression))
                    {
                        var targetMethodSymbolInfo = context.SemanticModel.GetSymbolInfo(node);
                        var targetMethodSymbol = targetMethodSymbolInfo.Symbol;
                        if (targetMethodSymbol is IMethodSymbol targetMethod &&
                            targetMethod.Name.StartsWith("Map", StringComparison.Ordinal) &&
                            SymbolEqualityComparer.Default.Equals(wellKnownTypes!.EndpointRouteBuilderExtensions, targetMethod.ContainingType) &&
                            targetMethod.Parameters.Length == 2 &&
                            SymbolEqualityComparer.Default.Equals(wellKnownTypes!.Delegate, targetMethod.Parameters[1].Type))
                        {
                            var invocation = (InvocationExpressionSyntax)node;
                            var arguments = invocation.ArgumentList.Arguments;
                            var routePattern = (ArgumentSyntax)arguments[0];
                            var routePatternText = ((LiteralExpressionSyntax)routePattern.Expression).Token.ValueText;
                            var parts = routePatternText.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            var prefix = parts[0];
                            if (encounteredRoutes.ContainsKey(prefix))
                            {
                                encounteredRoutes[prefix].Add(node.GetLocation());
                            }
                            else
                            {
                                encounteredRoutes[prefix] = new List<Location>() { node.GetLocation() };
                            }
                        }
                    }
                }

                foreach (var items in encounteredRoutes)
                {
                    if (items.Value.Count() == 1)
                    {
                        continue;
                    }
                    foreach (var location in items.Value)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            DiagnosticDescriptors.RecommendUsingRouteGroups,
                            location,
                            items.Key
                        ));
                    }
                }
            });

            context.RegisterOperationAction(context =>
            {
                var invocation = (IInvocationOperation)context.Operation;
                var invocationTarget = invocation.TargetMethod;
                if (invocationTarget is not null
                    && invocationTarget.Name == "AddJwtBearer"
                    && wellKnownTypes.JwtBearerExtensions is not null
                    && SymbolEqualityComparer.Default.Equals(wellKnownTypes!.JwtBearerExtensions, invocationTarget.ContainingType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseDotnetUserJwtsTool, invocation.Syntax.GetLocation()));
                }

            }, OperationKind.Invocation);
        });

        
    }
}

