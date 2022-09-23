using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MSHack2022.Codefixers
{
    internal sealed class WellKnownTypes
    // This is private. We initialize all the properties when TryCreate returns true.
    {
        public static bool TryCreate(Compilation compilation,
            [NotNullWhen(true)] out WellKnownTypes? wellKnownTypes,
            [NotNullWhen(false)] out string? failedType)
        {
            wellKnownTypes = default;
            const string HttpMethodAttribute = "Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute";
            if (compilation.GetTypeByMetadataName(HttpMethodAttribute) is not { } httpMethodAttribute)
            {
                failedType = HttpMethodAttribute;
                return false;
            }

            const string IHealthCheck = "Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck";
            if (compilation.GetTypeByMetadataName(IHealthCheck) is not { } iHealthCheck)
            {
                failedType = IHealthCheck;
                return false;
            }

            var checkHealthAsyncBaseMethod = (IMethodSymbol)iHealthCheck.GetMembers("CheckHealthAsync").Single();

            wellKnownTypes = new WellKnownTypes
            {
                HttpMethodAttribute = httpMethodAttribute,
                IHealthCheck = iHealthCheck,
                IHealthCheck_CheckHealthAsyncBaseMethod = checkHealthAsyncBaseMethod,
            };

            failedType = null;
            return true;
        }

        public INamedTypeSymbol HttpMethodAttribute { get; private set; } = null!;
        public INamedTypeSymbol IHealthCheck { get; private set; } = null!;
        public IMethodSymbol IHealthCheck_CheckHealthAsyncBaseMethod { get; private set; } = null!;
    }
}
