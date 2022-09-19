using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace MSHack2022.Analyzers
{
    internal static class RouteHandlerHelpers
    {
        internal const int DelegateParameterOrdinal = 2;

        internal static bool IsRouteHandlerInvocation(
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
}
