using CSharpToJavaScript;
using System.Collections.Generic;
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace CSTOJS_CLI;

public class Program
{
	private static int Main(string[] args)
	{
		RootCommand rootCommand = new("CLI for CSharpToJavaScript library.");

		//
		//Options CSharpToJavaScript
		Option<bool> debugOption = new("-Debug", "/Debug") 
		{
			DefaultValueFactory = (e) => { return false; },
			Description = "Debug. When set to true prints additional info to console, cs lines to js file."
		};

		Option<bool> disableConsoleColorsOption = new("-DisableConsoleColors", "/DisableConsoleColors") 
		{
			DefaultValueFactory = (e) => { return false; },
			Description = "Self-explanatory, Disable Console Colors."
		};

		Option<bool> disableConsoleOutputOption = new("-DisableConsoleOutput", "/DisableConsoleOutput")
		{
			DefaultValueFactory = (e) => { return false; },
			Description = "Self-explanatory, Disable Console Output."
		};

		Option<string> outPutPathOption = new("-OutPutPath", "/OutPutPath")
		{
			DefaultValueFactory = (e) => { return Directory.GetCurrentDirectory(); },
			Description = "Output path for javascript file."
		};

		Option<bool> useVarOverLetOption = new("-UseVarOverLet", "/UseVarOverLet")
		{
			DefaultValueFactory = (e) => { return false; },
			Description = "Self-explanatory, Use var over let."
		};

		Option<bool> keepBraceOnTheSameLine = new("-KeepBraceOnTheSameLine", "/KeepBraceOnTheSameLine") 
		{
			DefaultValueFactory = (e) => { return false; },
			Description = "Keep Brace '{' on the same line."
		};

		Option<bool> normalizeWhitespace = new("-NormalizeWhitespace", "/NormalizeWhitespace") 
		{
			DefaultValueFactory = (e) => { return false; },
			Description = "Self-explanatory, Normalize Whitespace."
		};

		Option<bool> useStrictEquality = new("-UseStrictEquality", "/UseStrictEquality") 
		{
			DefaultValueFactory =(e) => { return false; },
			Description = "Replace '==' with '===' and '!=' with '!=='."
		};

		Option<string> customCSNamesToJS = new("-CustomCSNamesToJS", "/CustomCSNamesToJS") 
		{
			DefaultValueFactory = (e) => { return string.Empty; },
			Description = "List of custom names to convert. Example: Console-console,WriteLine-log"
		};

		Argument<string> pathArgument = new("path") 
		{
			Description = "Full path."
		};

		//
		Command fileCommand = new("file", "Choose a cs file or the folder with cs files.");
		fileCommand.Aliases.Add("f");

		fileCommand.Arguments.Add(pathArgument);
		
		fileCommand.Options.Add(debugOption);
		fileCommand.Options.Add(disableConsoleColorsOption);
		fileCommand.Options.Add(disableConsoleOutputOption);
		fileCommand.Options.Add(outPutPathOption);
		fileCommand.Options.Add(useVarOverLetOption);
		fileCommand.Options.Add(keepBraceOnTheSameLine);
		fileCommand.Options.Add(normalizeWhitespace);
		fileCommand.Options.Add(useStrictEquality);
		fileCommand.Options.Add(customCSNamesToJS);
		//

		//
		Command continuousCommand = new("continuous", "Choose a cs file and generate it continuously by watching the cs file.");
		continuousCommand.Aliases.Add("c");

		continuousCommand.Arguments.Add(pathArgument);

		continuousCommand.Options.Add(debugOption);
		continuousCommand.Options.Add(disableConsoleColorsOption);
		continuousCommand.Options.Add(disableConsoleOutputOption);
		continuousCommand.Options.Add(outPutPathOption);
		continuousCommand.Options.Add(useVarOverLetOption);
		continuousCommand.Options.Add(keepBraceOnTheSameLine);
		continuousCommand.Options.Add(normalizeWhitespace);
		continuousCommand.Options.Add(useStrictEquality);
		continuousCommand.Options.Add(customCSNamesToJS);
		//


		rootCommand.Subcommands.Add(fileCommand);
		rootCommand.Subcommands.Add(continuousCommand);

		fileCommand.SetAction(GenerateFile);
		
		continuousCommand.SetAction(GenerateContinuously);

		ParseResult parseResult = rootCommand.Parse(args);
		return parseResult.Invoke();
}

	public static void GenerateContinuously(ParseResult parseResult)
	{
		string? file = parseResult.GetValue<string?>("path");

		if (file == null)
			return;

		CLICSTOJSOptions options = new()
		{
			Debug = parseResult.GetValue<bool>("-Debug"),
			DisableConsoleColors = parseResult.GetValue<bool>("-DisableConsoleColors"),
			DisableConsoleOutput = parseResult.GetValue<bool>("-DisableConsoleOutput"),
			OutPutPath = parseResult.GetValue<string>("-OutPutPath"),
			UseVarOverLet = parseResult.GetValue<bool>("-UseVarOverLet"),
			KeepBraceOnTheSameLine = parseResult.GetValue<bool>("-KeepBraceOnTheSameLine"),
			NormalizeWhitespace = parseResult.GetValue<bool>("-NormalizeWhitespace"),
			UseStrictEquality = parseResult.GetValue<bool>("-UseStrictEquality"),
			CustomCSNamesToJS = parseResult.GetValue<string>("-CustomCSNamesToJS")
		};

		CSTOJS _CSTOJS = InitiateCSTOJS(options);

		_CSTOJS.GenerateOneContinuously(file);

		Console.WriteLine("Press any button to stop wathing.");
		Console.ReadLine();

		_CSTOJS.StopWatching();
	}

	public static async Task GenerateFile(ParseResult parseResult)
	{
		string? file = parseResult.GetValue<string?>("path");

		if (file == null)
			return;

		CLICSTOJSOptions options = new() 
		{
			Debug  = parseResult.GetValue<bool>("-Debug"),
			DisableConsoleColors = parseResult.GetValue<bool>("-DisableConsoleColors"),
			DisableConsoleOutput = parseResult.GetValue<bool>("-DisableConsoleOutput"),
			OutPutPath = parseResult.GetValue<string>("-OutPutPath"),
			UseVarOverLet  = parseResult.GetValue<bool>("-UseVarOverLet"),
			KeepBraceOnTheSameLine  = parseResult.GetValue<bool>("-KeepBraceOnTheSameLine"),
			NormalizeWhitespace  = parseResult.GetValue<bool>("-NormalizeWhitespace"),
			UseStrictEquality = parseResult.GetValue<bool>("-UseStrictEquality"),
			CustomCSNamesToJS = parseResult.GetValue<string>("-CustomCSNamesToJS")
		};

		CSTOJS _CSTOJS = InitiateCSTOJS(options);

		await _CSTOJS.GenerateOneAsync(file);
	}

	private static CSTOJS InitiateCSTOJS(CLICSTOJSOptions options)
	{
		CSTOJS? _CSTOJS;

		if (!options.IsDefault())
		{
			string customNames = options.CustomCSNamesToJS.Replace(" ", "").Trim();

			List<Tuple<string, string>> _cstojsList = new();

			if (customNames != string.Empty)
			{
				string[] _localTuples = [];
				if (customNames.Contains(','))
				{
					_localTuples = customNames.Split(',');

					for (int i = 0; i < _localTuples.Length; i++)
					{
						string[] _local = _localTuples[i].Split('-');
						_cstojsList.Add(new(_local[0], _local[1]));
					}
				}
				else
				{
					if (customNames.Contains('-'))
					{
						string[] _local = customNames.Split('-');
						_cstojsList.Add(new(_local[0], _local[1]));
					}
				}
			}



			_CSTOJS = new(new()
			{
				Debug = options.Debug,

				DisableConsoleColors = options.DisableConsoleColors,

				OutPutPath = options.OutPutPath,

				UseVarOverLet = options.UseVarOverLet,
				KeepBraceOnTheSameLine = options.KeepBraceOnTheSameLine,
				NormalizeWhitespace = options.NormalizeWhitespace,
				UseStrictEquality = options.UseStrictEquality,

				CustomCSNamesToJS = _cstojsList
			});
		}
		else
			_CSTOJS = new();

		return _CSTOJS;
	}
}

public class CLICSTOJSOptions
{
	public bool Debug { get; set; } = false;
	public bool DisableConsoleColors { get; set; } = false;
	public bool DisableConsoleOutput { get; set; } = false;
	public string OutPutPath { get; set; } = Directory.GetCurrentDirectory();
	public bool UseVarOverLet { get; set; } = false;
	public bool KeepBraceOnTheSameLine { get; set; } = false;
	public bool NormalizeWhitespace { get; set; } = false;
	public bool UseStrictEquality { get; set; } = false;

	public string CustomCSNamesToJS { get; set; } = string.Empty;

	public bool IsDefault()
	{
		if (Debug != false ||
			DisableConsoleColors != false ||
			DisableConsoleOutput != false ||
			UseVarOverLet != false ||
			KeepBraceOnTheSameLine != false ||
			NormalizeWhitespace != false ||
			UseStrictEquality != false)
		{
			return false;
		}

		if (OutPutPath != Directory.GetCurrentDirectory())
			return false;

		if (CustomCSNamesToJS != string.Empty)
			return false;

		return true;
	}
}