//¯\_(ツ)_/¯
//#!/usr/local/share/dotnet/dotnet run
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

List<Tuple<string, string>> archs = new();
archs.Add(new("any", "false"));

//https://learn.microsoft.com/ru-ru/dotnet/core/rid-catalog
archs.Add(new("win-x64", "true"));
archs.Add(new("win-x86", "true"));
archs.Add(new("win-arm64", "true"));

archs.Add(new("linux-x64", "true"));
archs.Add(new("linux-musl-x64", "true"));
archs.Add(new("linux-musl-arm64", "true"));
archs.Add(new("linux-arm", "true"));
archs.Add(new("linux-arm64", "true"));
archs.Add(new("linux-bionic-arm64", "true"));

archs.Add(new("osx-x64", "true"));
archs.Add(new("osx-arm64", "true"));

ProcessStartInfo startInfo = new()
{
	FileName = "dotnet",
	UseShellExecute = false,
	RedirectStandardOutput = true,
	RedirectStandardError = true,
	CreateNoWindow = true
};
for (int i = 0; i < archs.Count; i++)
{
	Console.WriteLine($"Running: 'dotnet build for: {archs[i].Item1}'");
	startInfo.Arguments = $"build --configuration Release --property:OutputPath=./bin/binaries/CSTOJS_CLI-{archs[i].Item1}/ --runtime {archs[i].Item1} --self-contained {archs[i].Item2}";
	Process proc = new() { StartInfo = startInfo };
	proc.Start();
	Console.WriteLine(proc.StandardOutput.ReadToEnd());
	Console.WriteLine(proc.StandardError.ReadToEnd());
	proc.WaitForExit();
}

if (!Path.Exists("./bin/zip/"))
{
	Directory.CreateDirectory("./bin/zip/");
}
for (int i = 0; i < archs.Count; i++)
{
	string _directoryName = $"CSTOJS_CLI-{archs[i].Item1}";

	Console.WriteLine($"Zip binary: {_directoryName}");
	ZipFile.CreateFromDirectory($"./bin/binaries/{_directoryName}/", $"./bin/zip/{_directoryName}.zip");
}