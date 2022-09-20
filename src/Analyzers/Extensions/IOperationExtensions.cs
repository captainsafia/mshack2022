namespace Microsoft.CodeAnalysis;

internal static class IOperationExtensions
{
    public static TOperation? FirstAncestorOrSelf<TOperation>(this IOperation? operation)
        where TOperation : class, IOperation
    {
        while (operation is not null)
        {
            if (operation is TOperation result)
            {
                return result;
            }

            operation = operation.Parent;
        }

        return null;
    }
}
