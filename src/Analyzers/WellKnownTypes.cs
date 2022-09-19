using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers
{
    internal sealed class WellKnownTypes
    {
        public static bool TryCreate(Compilation compilation, out WellKnownTypes? wellKnownTypes)
        {
            wellKnownTypes = default;
            const string EndpointRouteBuilderExtensions = "Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions";
            if (compilation.GetTypeByMetadataName(EndpointRouteBuilderExtensions) is not { } endpointRouteBuilderExtensions)
            {
                return false;
            }

            const string Delegate = "System.Delegate";
            if (compilation.GetTypeByMetadataName(Delegate) is not { } @delegate)
            {
                return false;
            }

            const string IResult = "Microsoft.AspNetCore.Http.IResult";
            if (compilation.GetTypeByMetadataName(IResult) is not { } iResult)
            {
                return false;
            }

            wellKnownTypes = new WellKnownTypes
            {
                EndpointRouteBuilderExtensions = endpointRouteBuilderExtensions,
                Delegate = @delegate,
                IResult = iResult,
            };

            return true;
        }

        public INamedTypeSymbol EndpointRouteBuilderExtensions { get; private set; } = null!;
        public INamedTypeSymbol Delegate { get; private set; } = null!;
        public INamedTypeSymbol IResult { get; private set; } = null!;
    }
}
