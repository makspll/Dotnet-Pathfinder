# Routinator 9000
Utilities and experiments in inspecting web based .net assemblies.


# Usage
```
dotnet build test_assemblies/dotnet8
dotnet run --project src/ReflectionUtils.csproj **/bin/**/dotnet8.dll -o Text
```

# CodeQL Experiments
This repo includes experiments in using dataflow to find all conventional routes in a .net assembly. The codeql queries are in the `codeqltest` directory.

## Setup

- Download codeql-bundle and add the executable to your path (or use the full path to the executable from now on): https://github.com/github/codeql-action/releases
- Install the CodeQL extension in VSCode
- Run `codeql database create ./db --language=csharp --source-root=test_assemblies/dotnet8` to create the database
- Open the database and codeql queries in VSCode
- Launch the test query
