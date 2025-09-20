using CSharpToJavaScript;
using System.Collections.Generic;
using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Text;

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


		Command setupCommand = new("setup", "Setup cstojs project.");
		Argument<string> outputArgument = new("folder")
		{
			Description = "Output folder."
		};
		setupCommand.Arguments.Add(outputArgument);
		setupCommand.SetAction(SetupAction);

		Command translateCommand = new("translate", "Translate specified files in 'cstojs_options.xml'.");
		translateCommand.SetAction(TranslateAction);

		rootCommand.Subcommands.Add(setupCommand);
		rootCommand.Subcommands.Add(translateCommand);
		
		ParseResult parseResult = rootCommand.Parse(args);
		return parseResult.Invoke();
}

	public static void SetupAction(ParseResult result)
	{
		string folder = result.GetRequiredValue<string>("folder");

		Console.WriteLine("Running: 'dotnet new console -f net10.0'");
		ProcessStartInfo startInfo = new()
		{
			FileName = "dotnet",
			Arguments = "new console -f net10.0",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			CreateNoWindow = true
		};
		Process proc = new() { StartInfo = startInfo };
		proc.Start();
		proc.WaitForExit();

		Console.WriteLine("Adding CSharpToJavaScript package: 'dotnet add package CSharpToJavaScript'");
		startInfo.Arguments = "add package CSharpToJavaScript";
		proc.Start();
		proc.WaitForExit();

		Console.WriteLine($"Creating an output folder: '{folder}'");
		Directory.CreateDirectory(folder);

		Console.WriteLine($"Creating 'cstojs_options.xml'");
		XmlDocument doc = new();

		XmlElement root = doc.CreateElement(string.Empty, "ProjectOptions", string.Empty);

		XmlElement output = doc.CreateElement("Output");
		output.SetAttribute("Folder", "./" + folder);
		root.AppendChild(output);

		XmlElement file = doc.CreateElement("File");
		file.SetAttribute("Source", "./Program.cs");
		root.AppendChild(file);

		doc.AppendChild(root);
		doc.Save("cstojs_options.xml");

		Console.WriteLine($"Setup ended! Try running 'cstojs-cli translate'");
	}

	public static async Task TranslateAction(ParseResult result)
	{
		CSTOJS cstojs = new();
		
		CSTOJSOptions defaultOptions = new();
		string currentFile = string.Empty;

		string outputPath = string.Empty;

		Dictionary<string, CSTOJSOptions> files = new();
		
		string pathCombined = string.Empty;
		
		using (XmlReader reader = XmlReader.Create("cstojs_options.xml"))
		{
			while (reader.Read())
			{

				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						{
							Console.WriteLine("Start Element {0}", reader.Name);

							if (reader.Name == "Output")
							{
								outputPath = reader.GetAttribute(0);
								break;
							}
							if (reader.Name == "File")
							{
								currentFile = reader.GetAttribute("Source");
								if (reader.IsEmptyElement)
									files.Add(currentFile, defaultOptions);
								else
									files.Add(currentFile, new());
								break;
							}
							if (reader.Name == "Option")
							{
								string? _debug = reader.GetAttribute("Debug");

								if (currentFile == string.Empty)
								{
									if (_debug != null)
										defaultOptions.Debug = bool.Parse(_debug);
								}
								else
								{
									if (_debug != null)
										files[currentFile].Debug = bool.Parse(_debug);
								}
								break;
							}
							break;
						}
					case XmlNodeType.EndElement:
						{
							Console.WriteLine("End Element {0}", reader.Name);
							if (reader.Name == "File")
								currentFile = string.Empty;
							break;
						}
					default:
						Console.WriteLine($"-Other node: {reader.NodeType} | Value: {reader.Value}");
						break;
				}
			}
		}

		foreach (KeyValuePair<string, CSTOJSOptions> keyValue in files)
		{
			List<StringBuilder> _sb = cstojs.GenerateOne(keyValue.Key, keyValue.Value);
			pathCombined = Path.Combine(outputPath, keyValue.Key.Replace(".cs", ".js"));
			await File.WriteAllTextAsync(pathCombined, _sb[0].ToString());
			Console.WriteLine($"--- Path: {Path.GetFullPath(pathCombined)}");
		}
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

		CSTOJS _CSTOJS = new();

		CSTOJSOptions cstojsOptions = InitiateCSTOJS(options);

		_CSTOJS.GenerateOneContinuously(file, cstojsOptions);

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

		CSTOJS _CSTOJS = new();

		CSTOJSOptions cstojsOptions = InitiateCSTOJS(options);

		await _CSTOJS.GenerateOneAsync(file, cstojsOptions);
	}

	private static CSTOJSOptions InitiateCSTOJS(CLICSTOJSOptions options)
	{
		CSTOJSOptions? _opt;

		if (!options.IsDefault())
		{
			string customNames = options.CustomCSNamesToJS.Replace(" ", "").Trim();

			Dictionary<string, string> _cstojsList = new();

			if (customNames != string.Empty)
			{
				string[] _localTuples = [];
				if (customNames.Contains(','))
				{
					_localTuples = customNames.Split(',');

					for (int i = 0; i < _localTuples.Length; i++)
					{
						string[] _local = _localTuples[i].Split('-');
						_cstojsList.Add(_local[0], _local[1]);
					}
				}
				else
				{
					if (customNames.Contains('-'))
					{
						string[] _local = customNames.Split('-');
						_cstojsList.Add(_local[0], _local[1]);
					}
				}
			}



			_opt = new()
			{
				Debug = options.Debug,

				DisableConsoleColors = options.DisableConsoleColors,

				OutputPath = options.OutPutPath,

				UseVarOverLet = options.UseVarOverLet,
				KeepBraceOnTheSameLine = options.KeepBraceOnTheSameLine,
				NormalizeWhitespace = options.NormalizeWhitespace,
				UseStrictEquality = options.UseStrictEquality,

				CustomCSNamesToJS = _cstojsList
			};
		}
		else
			_opt = new();

		return _opt;
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
