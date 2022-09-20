using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace MSHack2022.Analyzers
{
    internal sealed class WellKnownTypes
    // This is private. We initialize all the properties when TryCreate returns true.
    {
        public static bool TryCreate(Compilation compilation,
            [NotNullWhen(true)] out WellKnownTypes? wellKnownTypes,
            [NotNullWhen(false)] out string? failedType)
        {
            wellKnownTypes = default;

            const string EndpointRouteBuilderExtensions = "Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions";
            if (compilation.GetTypeByMetadataName(EndpointRouteBuilderExtensions) is not { } endpointRouteBuilderExtensions)
            {
                failedType = EndpointRouteBuilderExtensions;
                return false;
            }

            const string UseExtensions = "Microsoft.AspNetCore.Builder.UseExtensions";
            if (compilation.GetTypeByMetadataName(UseExtensions) is not { } useExtensions)
            {
                failedType = UseExtensions;
                return false;
            }

            const string Delegate = "System.Delegate";
            if (compilation.GetTypeByMetadataName(Delegate) is not { } @delegate)
            {
                failedType = Delegate;
                return false;
            }

            const string Task = "System.Threading.Tasks.Task";
            if (compilation.GetTypeByMetadataName(Task) is not { } task)
            {
                failedType = Task;
                return false;
            }

            const string TaskTResult = "System.Threading.Tasks.Task`1";
            if (compilation.GetTypeByMetadataName(TaskTResult) is not { } taskTResult)
            {
                failedType = TaskTResult;
                return false;
            }

            const string FuncOfT1T2TResult = "System.Func`3";
            if (compilation.GetTypeByMetadataName(FuncOfT1T2TResult) is not { } funcOfT1T2TResult)
            {
                failedType = FuncOfT1T2TResult;
                return false;
            }

            const string RequestDelegate = "Microsoft.AspNetCore.Http.RequestDelegate";
            if (compilation.GetTypeByMetadataName(RequestDelegate) is not { } requestDelegate)
            {
                failedType = RequestDelegate;
                return false;
            }

            const string IServiceProvider = "System.IServiceProvider";
            if (compilation.GetTypeByMetadataName(IServiceProvider) is not { } iServiceProvider)
            {
                failedType = IServiceProvider;
                return false;
            }

            const string HttpContext = "Microsoft.AspNetCore.Http.HttpContext";
            if (compilation.GetTypeByMetadataName(HttpContext) is not { } httpContext)
            {
                failedType = HttpContext;
                return false;
            }

            const string ServiceProviderServiceExtensions = "Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions";
            if (compilation.GetTypeByMetadataName(ServiceProviderServiceExtensions) is not { } serviceProviderExtensions)
            {
                failedType = ServiceProviderServiceExtensions;
                return false;
            }

            const string JwtBearerExtensions = "Microsoft.Extensions.DependencyInjection.JwtBearerExtensions";
            if (compilation.GetTypeByMetadataName(JwtBearerExtensions) is not { } jwtBearerExtensions)
            {
                failedType = JwtBearerExtensions;
                return false;
            }

            const string EndpointNameAttribute = "Microsoft.AspNetCore.Routing.EndpointNameAttribute";
            if (compilation.GetTypeByMetadataName(EndpointNameAttribute) is not { } endpointNameAttribute)
            {
                failedType = EndpointNameAttribute;
                return false;
            }

            // Construct generic type symbols
            var funcOfHttpContextRequestDelegateTask = funcOfT1T2TResult.Construct(httpContext, requestDelegate, task);

            wellKnownTypes = new WellKnownTypes
            {
                EndpointRouteBuilderExtensions = endpointRouteBuilderExtensions,
                UseExtensions = useExtensions,
                Delegate = @delegate,
                RequestDelegate = requestDelegate,
                HttpContext = httpContext,
                Task = task,
                TaskTResult = taskTResult,
                FuncOfT1T2TResult = funcOfT1T2TResult,
                Func_HttpContext_RequestDelegate_Task = funcOfHttpContextRequestDelegateTask,
                IServiceProvider = iServiceProvider,
                ServiceProviderExtensions = serviceProviderExtensions,
                JwtBearerExtensions = jwtBearerExtensions,
                EndpointNameAttribute = endpointNameAttribute
            };

            failedType = null;
            return true;
        }

        public INamedTypeSymbol EndpointRouteBuilderExtensions { get; private set; } = null!;
        public INamedTypeSymbol UseExtensions { get; private set; } = null!;
        public INamedTypeSymbol Delegate { get; private set; } = null!;
        public INamedTypeSymbol RequestDelegate { get; private set; } = null!;
        public INamedTypeSymbol HttpContext { get; private set; } = null!;
        public INamedTypeSymbol Task { get; private set; } = null!;
        public INamedTypeSymbol TaskTResult { get; private set; } = null!;
        public INamedTypeSymbol FuncOfT1T2TResult { get; private set; } = null!;
        public INamedTypeSymbol Func_HttpContext_RequestDelegate_Task { get; private set; } = null!;
        public INamedTypeSymbol IServiceProvider { get; private set; } = null!;
        public INamedTypeSymbol ServiceProviderExtensions { get; private set; } = null!;
        public INamedTypeSymbol JwtBearerExtensions { get; private set; } = null!;
        public INamedTypeSymbol EndpointNameAttribute { get; private set; } = null!;
    }
}
