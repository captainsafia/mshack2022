using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace MSHack2022.Tests;

public static partial class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic();

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(descriptor);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
        {
            TestCode = source,
            // We need to set the output type to an exe to properly
            // support top-level programs in the tests. Otherwise,
            // the test infra will assume we are trying to build a library.
            TestState = { OutputKind = OutputKind.ConsoleApplication },
            ReferenceAssemblies = GetReferenceAssemblies(),
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync(CancellationToken.None);
    }

    internal static ReferenceAssemblies GetReferenceAssemblies()
    {
        return ReferenceAssemblies.Net.Net60.AddAssemblies(ImmutableArray.Create(
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Hosting.WebHostBuilderExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Hosting.IHostBuilder).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Hosting.HostingHostBuilderExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.ConfigureHostBuilder).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.ConfigureWebHostBuilder).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.EndpointRoutingApplicationBuilderExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Components.ComponentBase).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Components.ParameterAttribute).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Http.IResult).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Hosting.HostingAbstractionsWebHostBuilderExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Logging.ILoggingBuilder).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Logging.ConsoleLoggerExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.DependencyInjection.AntiforgeryServiceCollectionExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.FileProviders.IFileProvider).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Configuration.ConfigurationManager).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Configuration.JsonConfigurationExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Configuration.IConfigurationBuilder).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.Configuration.EnvironmentVariablesExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.DependencyInjection.JwtBearerExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.Extensions.DependencyInjection.AuthenticationServiceCollectionExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.JSInterop.IJSRuntime).Assembly.Location)));
    }

    static string TrimAssemblyExtension(string fullPath) => fullPath.Replace(".dll", string.Empty);
}