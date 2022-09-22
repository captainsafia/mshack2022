// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace MSHack2022.Tests;

public static partial class CSharpCodeRefactoringVerifier<TCodeRefactoring>
    where TCodeRefactoring : CodeRefactoringProvider, new()
{
    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, string)"/>
    public static Task VerifyRefactoringAsync(string source, string fixedSource)
    {
        return VerifyRefactoringAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);
    }

    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult, string)"/>
    public static Task VerifyRefactoringAsync(string source, DiagnosticResult expected, string fixedSource)
    {
        return VerifyRefactoringAsync(source, new[] { expected }, fixedSource);
    }

    /// <inheritdoc cref="CodeRefactoringVerifier{TCodeRefactoring, TTest, TVerifier}.VerifyRefactoringAsync(string, DiagnosticResult[], string)"/>
    public static Task VerifyRefactoringAsync(string source, DiagnosticResult[] expected, string fixedSource)
    {
        var test = new CSharpCodeRefactoringTest<TCodeRefactoring, XUnitVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource,
            ReferenceAssemblies = GetReferenceAssemblies(),
        };

        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync(CancellationToken.None);
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
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Builder.EndpointRouteBuilderExtensions).Assembly.Location),
            TrimAssemblyExtension(typeof(Microsoft.AspNetCore.Routing.RouteData).Assembly.Location),
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
