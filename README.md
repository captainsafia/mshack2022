# MSHack 2022 Analyzers

This repository contains a collection of analyzers developed during MSHack 2022, Microsoft's internal hackathon.

## Supported Diagnostics

| Diagnostic ID | Decription |
| ------------- | ---------- |
| MH001         | Just an easter egg analyzer. Finds instances where an integer variable is assigned to 42 and recommends renaming the identifier to `meaningOfLife`. |
| MH002         | Emits a warning when a Razor component writes to one of its parameter properties directly. |
| MH003         | Recommends that the user leverage the `dotnet user-jwts` command line tool when they are using JWT-bearer based auth. |
| MH005         | Finds 'out', 'in', or 'ref' modifiers on arguments in request delegates. |
| MH006         | Code fixer that adds a call to `WithName("SuggestedApiName")` to an endpoint mapping declaration |
| MH007         | Emits a warning when JS interop is performed within the 'OnInitializedAsync()' method in a Razor component. |
| MH008         | Recommends that users leverage route groups for multiple routes that share the same prefix. |
| MH009         | Warns if a for loop iterator variable is captured in a Blazor EventCallback. |
| MH011         | Errors when returning a by-ref value in a request delegate. |
| MH012         | Suggest users put RouteValues in request delegate arguments instead of accessing the dictionary. |
| MH013         | Warns when the `[SupplyParameterFromQuery]` or `[EditorRequired]` attributes are used without either the `[Parameter]` or `[CascadingParameter]` attributes. |
| MH014         | Code fixer and analyzer to make `[JSInvokable]` methods public. |

## Development Instructions

This repo contains the following directories:

- `/tests`: Contains tests for the analyzers and codefixers in the package. These tests leverage the official [Microsoft.CodeAnalysis.Testing](https://github.com/dotnet/roslyn-sdk/blob/main/src/Microsoft.CodeAnalysis.Testing/README.md) packages.
- `src/Analyzers`: Contains implementations for the analyzers in this repo.
- `src/Codefixers`: Contains implementations for codefixers associated with each diagnostic produced by the analyzers.
- `src/Package`: Contains MSBuild and NuSpec configuration for packaging the analyzers and codefixers into a NuGet package.

### Build Instructions

1. Clone the repo to your development machine using `git clone`.
2. To run the tests in the repo, navigate to the `tests` directory and use `dotnet test`.
3. To produce a NuGet package for the analyzers in this repo, navigate into the `src/Package` directory and run `dotnet pack`.

## Installation Instructions

The analyzers and codefixers in this repo are shipped in the [Hackalyzers](https://www.nuget.org/packages/Hackalyzers) package on NuGet. To get started with this analyzers, install the latest version of the package in your project of choice.

```
$ dotnet add package Hackalyzers
```

Note: you may need to restart any Visual Studio instances for the analyzers to take effect.
