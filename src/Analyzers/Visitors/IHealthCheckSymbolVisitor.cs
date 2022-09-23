using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace MSHack2022.Analyzers.Visitors
{
    internal class INamedTypeSymbolVisitor : SymbolVisitor
    {
        private readonly CancellationToken _cancellationToken;
        private readonly HashSet<INamedTypeSymbol> _exportedTypes;

        public INamedTypeSymbolVisitor(CancellationToken cancellation)
        {
            _cancellationToken = cancellation;
            _exportedTypes = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        }

        public ImmutableArray<INamedTypeSymbol> GetPublicTypes() => _exportedTypes.ToImmutableArray();

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            symbol.GlobalNamespace.Accept(this);
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            foreach (INamespaceOrTypeSymbol namespaceOrType in symbol.GetMembers())
            {
                _cancellationToken.ThrowIfCancellationRequested();
                namespaceOrType.Accept(this);
            }
        }

        public override void VisitNamedType(INamedTypeSymbol type)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (!_exportedTypes.Add(type))
                return;

            var nestedTypes = type.GetTypeMembers();
            if (nestedTypes.IsDefaultOrEmpty)
                return;

            foreach (INamedTypeSymbol nestedType in nestedTypes)
            {
                _cancellationToken.ThrowIfCancellationRequested();
                nestedType.Accept(this);
            }
        }
    }
}