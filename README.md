# CSTOJS_CLI
[Dotnet tool](https://www.nuget.org/packages/TiLied.CSTOJS_CLI/) | [Core library](https://github.com/TiLied/CSharpToJavaScript) | [Website](https://tilied.github.io/CSTOJS_Pages/) | [Try it online!](https://tilied.github.io/CSTOJS_Pages/BWA/)

This dotnet tool/CLI is a "front-end" that implements a "core" library [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) translator, converter, transpiler, transcompiler, compiler, or source-to-source compiler, you name it. 

The CLI was inspired (a little) by Meson, but should behave more or less like the dotnet cli or tsc cli.

## Quick start
### To install:
```csharp
dotnet tool install --global TiLied.CSTOJS_CLI
```
### To use:
```csharp
cstojs-cli setup "Output"
```
```csharp
cstojs-cli translate
```
### To update:
```csharp
dotnet tool update --global TiLied.CSTOJS_CLI
```
### To uninstall:
```csharp
dotnet tool uninstall --global TiLied.CSTOJS_CLI
```
## In-depth start
- [Dotnet 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) required.
-  To install dotnet tool globally run:
```csharp
dotnet tool install --global TiLied.CSTOJS_CLI
```
- - To install locally, follow [this tutorial](https://learn.microsoft.com/en-us/dotnet/core/tools/local-tools-how-to-use)
- - For linux update PATH, see [documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install#global-tools)
- Make a new directory "Test". Inside that directory, run:
```csharp
cstojs-cli setup "Output"
```
- - The command executes:
- - - dotnet new console -f net10.0
- - - dotnet add package CSharpToJavaScript
- - - Creating "Output" folder
- - - Creating "cstojs_options.xml"
- - Structure will be:
```csharp
- obj
- Output
- cstojs_options.xml
- Program.cs
- Test.csproj
```
- - The "Output" folder is where the translated JS files will be.
- - "cstojs_options.xml" is project options, see below for an example.
- To translate "Program.cs", run:
```csharp
cstojs-cli translate
```
- - The "Output" folder will contain a translated "Program.js" file.
- To add a new CS file, add `<File Source="./Test.cs" />` to "cstojs_options.xml" and run "cstojs-cli translate" again.
- To update, run:
```csharp
dotnet tool update --global TiLied.CSTOJS_CLI
```
- To uninstall, run:
```csharp
dotnet tool uninstall --global TiLied.CSTOJS_CLI
```

## cstojs_options.xml
```xml
<ProjectOptions>
  <!-- This specifies an output folder for js files. This example is "Output". -->
  <Output Folder="Output" />

  <!-- This is the default option which will be applied to every file that follows. -->
  <!-- See all available options at https://github.com/TiLied/CSharpToJavaScript/blob/master/CSharpToJavaScript/CSTOJSOptions.cs -->
  <Option NormalizeWhitespace="true" />

  <!-- This is a file that will be translated to js. -->
  <File Source="./Program.cs" />

  <!-- This is a file that will be translated to js with overridden options. -->
  <File Source="./Test.cs">
     <!-- This is the "Debug" option, applying only to this file. -->
    <Option Debug="true" />
  </File>

</ProjectOptions>
```

## Tutorials/Examples
- [Hello world](https://tilied.github.io/CSTOJS_Pages/tutorials/hello-world.html#hello-world)
- [Simple todo](https://tilied.github.io/CSTOJS_Pages/tutorials/simple-todo.html)
- [Simple module](https://tilied.github.io/CSTOJS_Pages/tutorials/simple-module.html)

[More on the website](https://tilied.github.io/CSTOJS_Pages/tutorials/hello-world.html)

## Commands
Run subcommands with `-h` to get more information.
```
init <folder>   Create a barebone 'cstojs_options.xml', without running the dotnet commands.
setup <folder>  Setup cstojs project.
translate       Translate specified files in the 'cstojs_options.xml'.
watch           Watches specified files in the 'cstojs_options.xml' with an interval and translates them. Note: The 'cstojs_options.xml' file is not being monitored, so any changes require the command to be restarted.
```

## Related Repository 
- Core library: https://github.com/TiLied/CSharpToJavaScript
- Tests: https://github.com/TiLied/CSTOJS_Tests
- Library for generating various things: https://github.com/TiLied/CSTOJS_GenLib
- Website/documentation: https://github.com/TiLied/CSTOJS_Pages
- Blazor WebAssembly app: https://github.com/TiLied/CSTOJS_BWA
