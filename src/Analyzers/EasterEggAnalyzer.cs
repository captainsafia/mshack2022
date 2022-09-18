using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSHack2022.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public partial class EasterEggAnalyzer : DiagnosticAnalyzer
{
     public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
        DiagnosticDescriptors.MeaningOfLife);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context =>
        {
            var localDeclarationStatement = (LocalDeclarationStatementSyntax)context.Node;
            var localDeclaration = (VariableDeclarationSyntax)localDeclarationStatement.Declaration;
            var variables = localDeclaration.Variables;
            foreach (var variable in variables)
            {
                var initializer = variable.Initializer;
                var identifierName = variable.Identifier;
                var value = initializer.Value;
                if (value is LiteralExpressionSyntax literalExpression
                    && literalExpression.Token.Text == "42"
                    && identifierName.Text != "meaningOfLife")
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MeaningOfLife, context.Node.GetLocation()));
                }
            }
        }, SyntaxKind.LocalDeclarationStatement);
    }
}