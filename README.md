# MSHack 2022 Analyzers

This repository contains a collection of analyzers developed during MSHack 2022, Microsoft's internal hackathon.

## Supported Diagnostics

| Diagnostic ID | Decription                                                                                                                                          |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| MH001         | Just an easter egg analyzer. Finds instances where an integer variable is assigned to 42 and recommends renaming the identifier to `meaningOfLife`. |
| MH002         | Emits a warning when a Razor component writes to one of its parameter properties directly.                                                          |
| MH007         | Emits a warning when JS interop is performed within the 'OnInitializedAsync()' method in a Razor component.                                         |

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
