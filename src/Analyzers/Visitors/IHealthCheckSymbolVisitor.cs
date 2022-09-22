using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers.Visitors
{
    internal class IHealthCheckSymbolVisitor
    {
        private readonly INamedTypeSymbol _iHealthCheckSymbol;

        public IHealthCheckSymbolVisitor(INamedTypeSymbol iHealthCheckSymbol)
        {
            _iHealthCheckSymbol = iHealthCheckSymbol;
        }

        public void VisitMethod(IMethodSymbol methodSymbol)
        {

        }
    }
}