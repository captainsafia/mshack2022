using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MSHack2022.Analyzers.Visitors
{
    public class HttpMethodSymbolVisitor
    {
        private readonly SemanticModel _semanticModel;

        public HttpMethodSymbolVisitor(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        ////NOTE: We have to visit the namespace's children even though
        ////we don't care about them. 😦
        //public override void VisitNamespace(INamespaceSymbol symbol)
        //{
        //    foreach (var child in symbol.GetMembers())
        //    {
        //        child.Accept(this);
        //    }
        //}

        ////NOTE: We have to visit the named type's children even though
        ////we don't care about them. 😦
        //public override void VisitNamedType(INamedTypeSymbol symbol)
        //{
        //    foreach (var child in symbol.GetMembers())
        //    {
        //        child.Accept(this);
        //    }
        //}

        public void VisitMethod(IMethodSymbol methodSymbol)
        {
            var methodDeclarationSyntaxNodes = methodSymbol.GetSyntaxNodes().Cast<MethodDeclarationSyntax>();
            foreach (var node in methodDeclarationSyntaxNodes)
            {
                if (_semanticModel.GetSymbolInfo(node).Symbol is IMethodSymbol innerMethodSymbol)
                {
                    var iHealthCheckVisitor = new IHealthCheckSymbolVisitor(null); // todo
                    iHealthCheckVisitor.VisitMethod(innerMethodSymbol);
                }

                
            }

            //Console.WriteLine(symbol);
        }
    }
}
