# Pathfinder
Utilities and experiments in inspecting routes in web based .net assemblies.


# Usage
```
dotnet build test_assemblies/dotnet8
dotnet run --project src/app **/bin/**/dotnet8.dll -o Text

Assembly: dotnet8.dll, Path: /home/makspll/git/reflection-utils/test_assemblies/dotnet8/bin/Debug/net8.0/dotnet8.dll
Controller: AttributeController
  - Get /attributecontrollerprefix
  - GetWithRoute /attributecontrollerprefix/getwithroute
  - MultipleRoutes /attributecontrollerprefix/multipleroutes/1
  - MultipleRoutes /attributecontrollerprefix/multipleroutes/2
Controller: AttributeControllerNoRoute
  - Get /getWithHttpGetRoute
  - GetWithRoute /getwithroute
Controller: InheritingController
  - Get /inheritingcontrollerprefix
  - GetWithRoute /inheritingcontrollerprefix/getwithroute
Controller: InheritingControllerNoRoute
  - GetWithRoute /getwithroute
Controller: InheritingController2
  - Get /inheritingcontroller2prefix
  - GetWithRoute /inheritingcontroller2prefix/getwithroute
Controller: InheritingController2NoRoute
  - GetWithRoute /getwithroute
Controller: ActiveControllerWithNoMethodRoute
  - HelloWorld /activeController
Controller: ListAllRoutesController
  - ListAllRoutes /api/allroutes
```

# CodeQL Experiments
This repo includes experiments in using dataflow to find all conventional routes in a .net assembly. The codeql queries are in the `codeqltest` directory.

## Setup

- Download codeql-bundle and add the executable to your path (or use the full path to the executable from now on): https://github.com/github/codeql-action/releases
- Install the CodeQL extension in VSCode
- Run `codeql database create ./db --language=csharp --source-root=test_assemblies/dotnet8` to create the database
- Open the database and codeql queries in VSCode
- Launch the test query
