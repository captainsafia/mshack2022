using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers;

internal sealed class WellKnownTypes
{
    public static bool TryCreate(Compilation compilation, out WellKnownTypes? wellKnownTypes)
    {
        wellKnownTypes = default;
        const string JwtBearerExtensions = "Microsoft.Extensions.DependencyInjection.JwtBearerExtensions";
        if (compilation.GetTypeByMetadataName(JwtBearerExtensions) is not { } jwtBearerExtensions)
        {
            return false;
        }

        wellKnownTypes = new WellKnownTypes()
        {
            JwtBearerExtensions = jwtBearerExtensions
        };

        return true;
    }

    public INamedTypeSymbol? JwtBearerExtensions { get; private set; }

}
