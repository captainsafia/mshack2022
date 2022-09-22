using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MSHack2022.Analyzers.Extensions
{
    public static class ContextExtensions
    {
        public static void Log(this SymbolAnalysisContext @this, string message)
        {
            @this.ReportDiagnostic(Diagnostic.Create(GetDescriptor(message), @this.Symbol.Locations.First()));
        }

        public static void Log(this CompilationAnalysisContext @this, string message)
        {
            @this.ReportDiagnostic(Diagnostic.Create(GetDescriptor(message), @this.Compilation.SyntaxTrees.First().GetRoot().GetLocation()));
        }

        public static DiagnosticDescriptor GetDescriptor(string message) => new(
               "MH014",
               message,
               message,
               "Usage",
               DiagnosticSeverity.Warning,
               isEnabledByDefault: true);
    }
}
