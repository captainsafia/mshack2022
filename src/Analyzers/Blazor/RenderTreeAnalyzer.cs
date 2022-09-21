using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Diagnostics;

namespace MSHack2022.Analyzers.Blazor;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RenderTreeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.AddKeyAttribute);

    public override void Initialize(AnalysisContext context)
    {
        Debugger.Launch();

        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterCompilationStartAction(compilationStartAnalysisContext =>
        {
            var compilation = compilationStartAnalysisContext.Compilation;

            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes, out var failedType))
            {
                //Debug.Fail($"{failedType} could not be found.");
                return;
            }

            compilationStartAnalysisContext.RegisterOperationAction(operationAnalysisContext =>
            {
                if (operationAnalysisContext.ContainingSymbol is not IMethodSymbol { Name: "BuildRenderTree" } methodSymbol)
                {
                    // TODO: Consider detecting if the current method is valid as a RenderFragment delegate.
                    return;
                }

                if (operationAnalysisContext.Operation is not ILoopOperation { LoopKind: LoopKind.For or LoopKind.ForEach } loopOperation)
                {
                    return;
                }

                // TODO: Handle case were it's not a block operation.
                if (loopOperation.Body is not IBlockOperation blockOperation)
                {
                    return;
                }

                var renderTreeStack = new Stack<RenderTreeElementType>();
                bool hasDefinedKeyAttribute = false;

                foreach (var childOperation in blockOperation.Operations)
                {
                    if (childOperation is not IExpressionStatementOperation expressionStatementOperation ||
                        expressionStatementOperation.Operation is not IInvocationOperation invocationOperation)
                    {
                        continue;
                    }

                    var operation = GetRenderTreeBuilderOperation(invocationOperation.TargetMethod, wellKnownTypes);

                    switch (operation)
                    {
                        case OpenOperation(var elementType):
                            renderTreeStack.Push(elementType);
                            break;
                        case CloseOperation(var elementType):
                            if (renderTreeStack.Count == 1)
                            {
                                if (!hasDefinedKeyAttribute)
                                {
                                    operationAnalysisContext.ReportDiagnostic(Diagnostic.Create(
                                        DiagnosticDescriptors.AddKeyAttribute,
                                        loopOperation.Syntax.GetLocation()));
                                    return;
                                }
                                hasDefinedKeyAttribute = false;
                            }

                            if (renderTreeStack.Count > 0 && renderTreeStack.Peek() == elementType)
                            {
                                renderTreeStack.Pop();
                            }
                            break;
                        case SetKeyOperation:
                            if (renderTreeStack.Count == 1)
                            {
                                hasDefinedKeyAttribute = true;
                            }
                            break;
                        default:
                            continue;
                    }
                }

                _ = 0;
            }, OperationKind.Loop);
        });
    }

    private static IRenderTreeOperation? GetRenderTreeBuilderOperation(
        IMethodSymbol methodSymbol,
        WellKnownTypes wellKnownTypes)
    {
        if (!SymbolEqualityComparer.Default.Equals(wellKnownTypes.RenderTreeBuilder, methodSymbol.OriginalDefinition.ContainingType))
        {
            return null;
        }

        return methodSymbol.Name switch
        {
            "OpenComponent" => new OpenOperation(RenderTreeElementType.Component),
            "OpenElement" => new OpenOperation(RenderTreeElementType.Element),
            "CloseComponent" => new CloseOperation(RenderTreeElementType.Component),
            "CloseElement" => new CloseOperation(RenderTreeElementType.Element),
            "SetKey" => new SetKeyOperation(),
            _ => null,
        };
    }

    private interface IRenderTreeOperation { }
    private record OpenOperation(RenderTreeElementType ElementType) : IRenderTreeOperation;
    private record CloseOperation(RenderTreeElementType ElementType) : IRenderTreeOperation;
    private record SetKeyOperation : IRenderTreeOperation;

    private enum RenderTreeElementType
    {
        Element = 0,
        Component = 1,
    }
}
