namespace Microsoft.CodeAnalysis.Operations;

internal static class IInvocationOperationExtensions
{
    public static ITypeSymbol? GetInstanceType(this IInvocationOperation invocation)
    {
        if (invocation.Instance?.Type is { } type)
        {
            return type;
        }

        var targetMethod = invocation.TargetMethod;

        if (targetMethod.IsExtensionMethod && targetMethod.Parameters.Length > 0)
        {
            return targetMethod.Parameters[0].Type;
        }

        return null;
    }
}
