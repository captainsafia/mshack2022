using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers
{
    internal sealed class WellKnownTypes
    // This is private. We initialize all the properties when TryCreate returns true.
    {
        public static bool TryCreate(Compilation compilation, [NotNullWhen(true)] out WellKnownTypes? wellKnownTypes)
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

            const string IServiceProvider = "System.IServiceProvider";
            if (compilation.GetTypeByMetadataName(IServiceProvider) is not { } iServiceProvider)
            {
                return false;
            }

            const string ServiceProviderServiceExtensions = "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions";
            if (compilation.GetTypeByMetadataName(ServiceProviderServiceExtensions) is not { } serviceProviderExtensions)
            {
                return false;
            }

            const string JwtBearerExtensions = "Microsoft.Extensions.DependencyInjection.JwtBearerExtensions";
            if (compilation.GetTypeByMetadataName(JwtBearerExtensions) is not { } jwtBearerExtensions)
            {
                return false;
            }

            wellKnownTypes = new WellKnownTypes
            {
                EndpointRouteBuilderExtensions = endpointRouteBuilderExtensions,
                Delegate = @delegate,
                IServiceProvider = iServiceProvider,
                ServiceProviderExtensions = serviceProviderExtensions,
                JwtBearerExtensions = jwtBearerExtensions,
            };

            return true;
        }

        public INamedTypeSymbol EndpointRouteBuilderExtensions { get; private set; } = null!;
        public INamedTypeSymbol Delegate { get; private set; } = null!;
        public INamedTypeSymbol IServiceProvider { get; private set; } = null!;
        public INamedTypeSymbol ServiceProviderExtensions { get; private set; } = null!;
        public INamedTypeSymbol JwtBearerExtensions { get; private set; } = null!;
    }
}
