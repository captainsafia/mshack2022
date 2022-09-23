using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers.Extensions
{
    public static class NamedTypeSymbolFinder
    {
        public static List<INamedTypeSymbol> GetAllSymbols(Compilation compilation, Func<INamedTypeSymbol, bool>? predicate = default)
        {
            var visitor = new FindAllSymbolsVisitor(predicate);
            visitor.Visit(compilation.GlobalNamespace);
            return visitor.AllTypeSymbols;
        }

        private class FindAllSymbolsVisitor : SymbolVisitor
        {
            private readonly Func<INamedTypeSymbol, bool>? _predicate;

            public FindAllSymbolsVisitor(Func<INamedTypeSymbol, bool>? predicate)
            {
                _predicate = predicate;
            }

            public List<INamedTypeSymbol> AllTypeSymbols { get; } = new List<INamedTypeSymbol>();

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (_predicate == null || _predicate.Invoke(symbol))
                {
                    AllTypeSymbols.Add(symbol);
                }

                foreach (var childSymbol in symbol.GetTypeMembers())
                {
                    base.Visit(childSymbol);
                }
            }
        }
    }
}
