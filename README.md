# CSTOJS_CLI
[Dotnet tool](https://www.nuget.org/packages/TiLied.CSTOJS_CLI/) | [Core library](https://github.com/TiLied/CSharpToJavaScript) | [Website](https://tilied.github.io/CSTOJS_Pages/) | [Try it online!](https://tilied.github.io/CSTOJS_Pages/BWA/)

This dotnet tool/CLI is a "front-end" that implements a "core" library [CSharpToJavaScript](https://github.com/TiLied/CSharpToJavaScript) translator, converter, transpiler, transcompiler, compiler, source-to-source compiler, you name it. CLI inspired (a little) by Meson, but it should behave like the dotnet cli and tsc cli.

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
### To delete:
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
- - "Output" folder is where the translated JS files will be.
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
- To delete, run:
```csharp
dotnet tool uninstall --global TiLied.CSTOJS_CLI
```

## cstojs_options.xml
```xml
<ProjectOptions>
  <!-- This is specifying an output folder for a js files. This example is "Output". -->
  <Output Folder="Output" />

  <!-- This is a default options which will be applied for every file that follows. -->
  <DefaultOptions>
    <!-- This is a "NormalizeWhitespace" option. See all available options at https://github.com/TiLied/CSharpToJavaScript/blob/master/CSharpToJavaScript/CSTOJSOptions.cs -->
    <Option NormalizeWhitespace="true" />
  </DefaultOptions>

  <!-- This is a file that will be translated to js. -->
  <File Source="./Program.cs" />

  <!-- This is a file that will be translated to js with overridden options. -->
  <File Source="./Test.cs">
     <!-- This is a "Debug" option applying to only this file. -->
    <Option Debug="true" />
  </File>

</ProjectOptions>
```

## Tutorials/Examples
- [Hello world](https://tilied.github.io/CSTOJS_Pages/tutorials/hello-world.html#hello-world)
- [Simple todo](https://tilied.github.io/CSTOJS_Pages/tutorials/simple-todo.html)

[More on a website](https://tilied.github.io/CSTOJS_Pages/tutorials/hello-world.html)

## Commands
Run subcommands with `-h` to get more information.
```
setup <folder>  Setup cstojs project.
translate       Translate specified files in 'cstojs_options.xml'.
```

## Related Repository 
- Core library: https://github.com/TiLied/CSharpToJavaScript
- Tests: https://github.com/TiLied/CSTOJS_Tests
- Library for generating various stuff: https://github.com/TiLied/CSTOJS_GenLib
- Website/documentation: https://github.com/TiLied/CSTOJS_Pages
- Blazor WebAssembly app: https://github.com/TiLied/CSTOJS_BWA
