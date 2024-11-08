# Pathfinder
<p align="center">
    <img src="https://i.imgur.com/TEDoP2L.jpeg" alt="Pathfinder" width="300"/>
</p>

Finds and outputs all API routes found in a .NET assembly in textual or JSON format.

## Features
- Attribute based routing
- Conventional routing (templates + defaults have to be specified in a config file)
- .NET core support
- .NET framework support
- JSON and Text output
- Configurable backing lib for projects with custom routing mechanisms
- Customizable template based report generation (see [report for assemblies in /test_assemblies](https://makspll.github.io/Pathfinder/))

# Installation

## Prerequisites
- .NET 7 or later installed (only for running CLI, not in your project)

## CLI
- `dotnet tool install -g Makspll.Pathfinder`

# Usage
```
pathfinder help

echo "Analyze and report on a dll"
pathfinder analyze **/bin/**/yourdllname.dll
pathfinder report **/bin/**/yourdllname.dll
```

![image](https://i.imgur.com/2Oz4HJA.png)

# Configuration

## Config file
The program is configured via `pathfinder.json` files found in your project. If the file is not found you can specify a path via the `-c` flag.

Currently the file needs to specify all your conventional routing configuration (anything that isn't attribute based).

### .NET framework
In .NET framework projects, you will need to specify whether each of your routes is an MVC or API route. This is done by adding a `Type` field to each route in the config file.

```json
{
    "ConventionalRoutes": [
        {
            "Template": "conventionalprefix/{controller}/{action}",
            "Type": "MVC"
        },
        {
            "Template": "conventionalprefix2/{controller}",
            "Defaults": {
                "action": "DefaultAction"
            },
            "Type": "MVC"
        },
        {
            "Template": "conventionalwithnoactionspecs",
            "Defaults": {
                "controller": "DefaultConventional",
                "action": "DefaultAction"
            },
            "Type": "MVC"
        },
        {
            "Template": "apiconventionalprefix/{controller}/{action}",
            "Type": "API"
        },
        {
            "Template": "apiconventionalprefix2/{controller}",
            "Defaults": {
                "action": "DefaultAction"
            },
            "Type": "API"
        },
        {
            "Template": "apiconventionalwithnoactionspecs",
            "Defaults": {
                "controller": "ApiDefaultConventionalApi",
                "action": "DefaultAction"
            },
            "Type": "API"
        }
    ]
}
```

### .NET core

.NET core does not make such a distinction, you shouldn't specify the type of controller:

```json
{
    "ConventionalRoutes": [
        {
            "Template": "conventionalprefix/{controller}/{action}"
        },
        {
            "Template": "conventionalprefix2/{controller}",
            "Defaults": {
                "action": "DefaultAction"
            }
        },
        {
            "Template": "conventionalwithnoactionspecs",
            "Defaults": {
                "controller": "DefaultConventional",
                "action": "DefaultAction"
            }
        }
    ]
}
```