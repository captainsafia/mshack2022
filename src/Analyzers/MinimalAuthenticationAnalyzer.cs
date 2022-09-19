using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class MinimalNet7Analyzers : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.UseDotnetUserJwtsTool);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterOperationAction(static context =>
        {
            var compilation = context.Compilation;
            if (!WellKnownTypes.TryCreate(compilation, out var wellKnownTypes))
            {
                Debug.Fail("One or more types could not be found. This usually means you are bad at spelling C# type names.");
                return;
            }

            var invocation = (IInvocationOperation)context.Operation;
            var invocationTarget = invocation.TargetMethod;
            if (invocationTarget is not null
                && invocationTarget.Name == "AddJwtBearer"
                && SymbolEqualityComparer.Default.Equals(wellKnownTypes!.JwtBearerExtensions, invocationTarget.ContainingType))
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.UseDotnetUserJwtsTool, invocation.Syntax.GetLocation()));
            }

        }, OperationKind.Invocation);
    }
}

