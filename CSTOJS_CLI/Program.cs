using CSharpToJavaScript;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Text;
using CSharpToJavaScript.Utils;
using System;
using System.Threading;

namespace CSTOJS_CLI;

public class Program
{
	public static bool RunWatch = true;

	private static int Main(string[] args)
	{
		RootCommand rootCommand = new("Dotnet tool/cli frontend for a CSharpToJavaScript library.");

		Option<bool> disableConsoleOutput = new("--disable-console-output")
		{
			Recursive = true,
			Description = "Self-explanatory, Disable Console Output.",
			DefaultValueFactory = (r) => { return false; }
		};
		rootCommand.Options.Add(disableConsoleOutput);

		Option<bool> disableConsoleColors = new("--disable-console-colors")
		{
			Recursive = true,
			Description = "Self-explanatory, Disable Console Colors.",
			DefaultValueFactory = (r) => { return false; }
		};
		rootCommand.Options.Add(disableConsoleColors);

		Argument<string> outputArgument = new("folder")
		{
			Description = "Output folder. Can be absolute path or relative."
		};

		Command initCommand = new("init", @"Creates a barebone 'cstojs_options.xml'(name can be changed with '--name' option) in the current directory, without running the dotnet commands.
This command should be used if a C# project already exists.");
		initCommand.Arguments.Add(outputArgument);
		initCommand.SetAction(InitAction);

		Option<string> nameOption = new("--name", "-n")
		{
			HelpName = "fileName",
			Description = @"The name of a project file, without the extension.",
			DefaultValueFactory = (r) => { return "cstojs_options"; }
		};
		initCommand.Options.Add(nameOption);
		
		Option<string[]> fileSourceOption = new("--file-source", "-fs")
		{
			HelpName = "paths",
			Description = "Adds file sources(paths) to the project file. Multiple paths can be specified.",
			AllowMultipleArgumentsPerToken = true,
			DefaultValueFactory = (r) => { return []; }
		};
		initCommand.Options.Add(fileSourceOption);
		
		Command setupCommand = new("setup", @"Setup cstojs project in the current directory.
This command should be used if you are starting from scratch.");
		setupCommand.Arguments.Add(outputArgument);
		setupCommand.SetAction(SetupAction);

		Option<string> projectPath = new("--project", "-p")
		{
			HelpName = "path",
			Description = @"Path to the 'cstojs_options.xml'. 
Note: The name of the file can be anything as long as the file ends with xml and follows structure `<ProjectOptions>...</ProjectOptions>`.",
			DefaultValueFactory = (r) => { return "./cstojs_options.xml"; },
			Validators =
			{
				(result) =>
				{
					if (!File.Exists(result.GetValue<string>("--project")))
						result.AddError($"Path to the project file does not exists: '{result.GetValue<string>("--project")}'");
				}
			}
		};

		Command translateCommand = new("translate", "Translate specified files in the 'cstojs_options.xml'.");
		translateCommand.SetAction(TranslateAction);
		translateCommand.Options.Add(projectPath);

		Option<int> delayWatch = new("--delay", "-d")
		{
			HelpName = "ms",
			Description = "Delay watching the files again by milliseconds. (1000-10000)",
			DefaultValueFactory = (r) => { return 3000; },
			Validators =
			{
				(result) =>
				{
					if (result.GetValue<int>("--delay") < 1000)
						result.AddError("Must be greater than 1000 ms.");
					else if (result.GetValue<int>("--delay") > 10000)
						result.AddError("Must be smaller than 10000 ms.");
				}
			}
		};

		Command watchCommand = new("watch", @"Watches specified files in the 'cstojs_options.xml' with an interval and translates them. 
Note: The 'cstojs_options.xml' file is not being monitored, so any changes require the command to be restarted.");
		watchCommand.SetAction(WatchAction);
		watchCommand.Options.Add(projectPath);
		watchCommand.Options.Add(delayWatch);

		//
		//configure commands/options:
		Command configureCommand = new("configure", @"Configure the 'cstojs_options.xml' without editing the xml by hand.");
		configureCommand.Options.Add(projectPath);

		Option<string> fileOption = new("--file")
		{
			HelpName = "file-source",
			Description = "Represents a File element where an Option element is located in the xml."
		};
		Command optionCommand = new("option", "Represent a Option element in the xml.");
		optionCommand.SetAction(OptionAction);
		configureCommand.Subcommands.Add(optionCommand);
		optionCommand.Options.Add(fileOption);

		//CSharpToJavaScript options:
		Option<bool?> Debug = new("--Debug")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> DisableDiagnostics = new("--DisableDiagnostics")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> UseVarOverLet = new("--UseVarOverLet")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> KeepBraceOnTheSameLine = new("--KeepBraceOnTheSameLine")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> NormalizeWhitespace = new("--NormalizeWhitespace")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> MakePropertiesEnumerable = new("--MakePropertiesEnumerable")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> DisableTranslation = new("--DisableTranslation")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<bool?> DisableNameOfProcessing = new("--DisableNameOfProcessing")
		{
			DefaultValueFactory = (r) => { return null; }
		};

		Option<byte?> EnableModules = new("--EnableModules")
		{
			DefaultValueFactory = (r) => { return null; }
		};

		Option<string?> CustomPathToDLLs = new("--CustomPathToDLLs")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<string?> CustomCSNamesToJS = new("--CustomCSNamesToJS")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<string?> AddSBAtTheTop = new("--AddSBAtTheTop")
		{
			DefaultValueFactory = (r) => { return null; }
		};
		Option<string?> AddSBAtTheBottom = new("--AddSBAtTheBottom")
		{
			DefaultValueFactory = (r) => { return null; }
		};

		optionCommand.Options.Add(Debug);
		optionCommand.Options.Add(DisableDiagnostics);
		optionCommand.Options.Add(UseVarOverLet);
		optionCommand.Options.Add(KeepBraceOnTheSameLine);
		optionCommand.Options.Add(NormalizeWhitespace);
		optionCommand.Options.Add(MakePropertiesEnumerable);
		optionCommand.Options.Add(DisableTranslation);
		optionCommand.Options.Add(DisableNameOfProcessing);

		optionCommand.Options.Add(EnableModules);

		optionCommand.Options.Add(CustomPathToDLLs);
		optionCommand.Options.Add(CustomCSNamesToJS);
		optionCommand.Options.Add(AddSBAtTheTop);
		optionCommand.Options.Add(AddSBAtTheBottom);
		//


		Command fileCommand = new("file", "Represent a File element in the xml.");
		fileCommand.SetAction(FileAction);
		configureCommand.Subcommands.Add(fileCommand);
		
		Argument<string> fileSourceArgument = new("file-source")
		{
			Description = $"The path to the cs file.",
		};
		fileCommand.Arguments.Add(fileSourceArgument);
		
		Option<bool> RemoveOption = new("--remove")
		{
			Description = @"Removes from the project file.",
			DefaultValueFactory = (r) => { return false; }
		};
		Option<bool> AddOption = new("--add")
		{
			Description = @"Adds to the project file.",
			DefaultValueFactory = (r) => { return false; }
		};
		Option<string> ChangeOption = new("--change")
		{
			Description = @"Changes file/option element in the project file.",
			DefaultValueFactory = (r) => { return ""; }
		};
		
		fileCommand.Options.Add(AddOption);
		fileCommand.Options.Add(RemoveOption);
		fileCommand.Options.Add(ChangeOption);

		optionCommand.Options.Add(AddOption);
		optionCommand.Options.Add(RemoveOption);
		optionCommand.Options.Add(ChangeOption);
		//

		rootCommand.Subcommands.Add(initCommand);
		rootCommand.Subcommands.Add(setupCommand);
		rootCommand.Subcommands.Add(translateCommand);
		rootCommand.Subcommands.Add(watchCommand);
		rootCommand.Subcommands.Add(configureCommand);
		
		ParseResult parseResult = rootCommand.Parse(args);
		return parseResult.Invoke();
	}

	public static void InitAction(ParseResult result)
	{
		SetupLog(result);
		
		string folder = result.GetRequiredValue<string>("folder");
		string name = result.GetValue<string>("--name") ?? "cstojs_options";
		string[] fileSources = result.GetValue<string[]>("--file-source") ?? [];
		
		if (File.Exists($"./{name}.xml"))
		{
			Log.ErrorLine($"'{name}.xml' already exists!");
			return;
		}

		if (Directory.Exists(Path.GetFullPath(folder)))
		{
			Log.InfoLine($"The specified output path exists, skipping creation: '{Path.GetFullPath(folder)}'");
		}
		else
		{
			Log.InfoLine($"Creating an output folder: '{Path.GetFullPath(folder)}'");
			Directory.CreateDirectory(folder);
		}
		
		Log.InfoLine($"Creating '{name}.xml'");
		XmlDocument doc = new();

		XmlElement root = doc.CreateElement(string.Empty, "ProjectOptions", string.Empty);

		XmlElement output = doc.CreateElement("Output");
		output.SetAttribute("Folder", folder);
		root.AppendChild(output);
		
		for (int i = 0; i < fileSources.Length; i++)
		{
			if (File.Exists(fileSources[i]))
			{
				XmlElement file = doc.CreateElement("File");
				file.SetAttribute("Source", fileSources[i]);
				root.AppendChild(file);
			}
			else
			{
				Log.WarningLine($"File does not exists: '{fileSources[i]}'");
			}
		}
		
		doc.AppendChild(root);
		doc.Save($"{name}.xml");

		Log.InfoLine($"Init ended!");
	}
	public static void SetupAction(ParseResult result)
	{
		SetupLog(result);
		
		if (File.Exists("./cstojs_options.xml"))
		{
			Log.ErrorLine($"'cstojs_options.xml' already exists!");
			return;
		}

		string folder = result.GetRequiredValue<string>("folder");

		Log.InfoLine("Running: 'dotnet new console -f net10.0'");
		ProcessStartInfo startInfo = new()
		{
			FileName = "dotnet",
			Arguments = "new console -f net10.0",

			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			CreateNoWindow = true
		};
		Process proc = new() { StartInfo = startInfo };
		proc.Start();
		Log.WriteLine(proc.StandardOutput.ReadToEnd());
		Log.WriteLine(proc.StandardError.ReadToEnd());
		proc.WaitForExit();

		Log.InfoLine("Adding CSharpToJavaScript package: 'dotnet add package CSharpToJavaScript'");
		startInfo.Arguments = "add package CSharpToJavaScript";
		proc.Start();
		Log.WriteLine(proc.StandardOutput.ReadToEnd());
		Log.WriteLine(proc.StandardError.ReadToEnd());
		proc.WaitForExit();

		if (Directory.Exists(Path.GetFullPath(folder)))
		{
			Log.InfoLine($"The specified output path exists, skipping creation: '{Path.GetFullPath(folder)}'");
		}
		else
		{
			Log.InfoLine($"Creating an output folder: '{Path.GetFullPath(folder)}'");
			Directory.CreateDirectory(folder);
		}

		Log.InfoLine($"Creating 'cstojs_options.xml'");
		XmlDocument doc = new();

		XmlElement root = doc.CreateElement(string.Empty, "ProjectOptions", string.Empty);

		XmlElement output = doc.CreateElement("Output");
		output.SetAttribute("Folder", folder);
		root.AppendChild(output);

		XmlElement file = doc.CreateElement("File");
		file.SetAttribute("Source", "./Program.cs");
		root.AppendChild(file);

		doc.AppendChild(root);
		doc.Save("cstojs_options.xml");

		Log.InfoLine($"Setup ended! Try running 'cstojs-cli translate'");
	}

	private static XMLData ReadXML(ParseResult result)
	{
		XMLData data = new();

		string projectPath = result.GetValue<string>("--project") ?? "./cstojs_options.xml";

		string directoryPath = Path.GetDirectoryName(Path.GetFullPath(projectPath)) ?? "./";
		CSTOJSOptions defaultOptions = new();

		FileData2? currentFile = null;

		string pathCombined = string.Empty;

		//CSTOJS Options:
		string? Debug = null;
		string? DisableDiagnostics = null;
		string? UseVarOverLet = null;
		string? KeepBraceOnTheSameLine = null;
		string? NormalizeWhitespace = null;
		string? MakePropertiesEnumerable = null;
		string? DisableTranslation = null;
		string? DisableNameOfProcessing = null;
		
		string? EnableModules = null;

		string? CustomPathToDLLs = null;
		string? CustomCSNamesToJS = null;
		string? AddSBAtTheTop = null;
		string? AddSBAtTheBottom = null;

		using (XmlReader reader = XmlReader.Create($"{projectPath}"))
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						{
							//Log.WriteLine($"Start Element {reader.Name}");

							if (reader.Depth == 0 && reader.Name != "ProjectOptions")
							{
								Log.ErrorLine("Root must be 'ProjectOptions'.");
								data.Error = true;
								return data;
							}
							
							if (reader.Name == "Output")
							{
								string? _output = reader.GetAttribute("Folder");

								if (_output == null)
								{
									Log.ErrorLine("Folder attribute is null!");
									data.Error = true;
									return data;
								}

								if (Path.IsPathRooted(_output))
								{
									if (!Directory.Exists(Path.GetFullPath(_output)))
									{
										Log.ErrorLine($"Directory does not exists: {_output}");
										data.Error = true;
										return data;
									}
									data.OutputPath = Path.GetFullPath(_output);
								}
								else
								{
									if (!Directory.Exists(Path.Combine(directoryPath, _output)))
									{
										Log.ErrorLine($"Directory does not exists: {Path.Combine(directoryPath, _output)}");
										data.Error = true;
										return data;
									}

									data.OutputPath = Path.Combine(directoryPath, _output);
								}
								break;
							}
							if (reader.Name == "File")
							{
								string? _source = reader.GetAttribute("Source");

								string _sourceFileName = string.Empty;
								string _sourceStr = string.Empty;
								string _sourceFilePath = string.Empty;

								if (_source == null)
								{
									Log.ErrorLine("Source attribute is null!");
									data.Error = true;
									return data;
								}


								if (Path.IsPathRooted(_source))
								{
									if (!File.Exists(_source))
									{
										Log.ErrorLine($"File does not exists: {_source}");
										data.Error = true;
										return data;
									}
									_sourceFileName = Path.GetFileName(_source);
									_sourceStr = File.ReadAllText(_source);
									_sourceFilePath = _source;
								}
								else
								{
									if (!File.Exists(Path.Combine(directoryPath, _source)))
									{
										Log.ErrorLine($"File does not exists: {Path.Combine(directoryPath, _source)}");
										data.Error = true;
										return data;
									}
									_sourceFileName = Path.GetFileName(Path.Combine(directoryPath, _source));
									_sourceStr = File.ReadAllText(Path.Combine(directoryPath, _source));
									_sourceFilePath = Path.Combine(directoryPath, _source);
								}

								currentFile = new()
								{
									FileName = _sourceFileName.Replace(".cs", ".js"),
									SourceStr = _sourceStr,
									SourceFilePath = _sourceFilePath
								};

								if (reader.IsEmptyElement)
									currentFile.OptionsForFile = defaultOptions;
								else
									currentFile.OptionsForFile = new();

								data.Files.Add(currentFile);
								break;
							}
							if (reader.Name == "Option")
							{
								Debug = reader.GetAttribute("Debug");
								DisableDiagnostics = reader.GetAttribute("DisableDiagnostics");
								UseVarOverLet = reader.GetAttribute("UseVarOverLet");
								KeepBraceOnTheSameLine = reader.GetAttribute("KeepBraceOnTheSameLine");
								NormalizeWhitespace = reader.GetAttribute("NormalizeWhitespace");
								MakePropertiesEnumerable = reader.GetAttribute("MakePropertiesEnumerable");
								DisableTranslation = reader.GetAttribute("DisableTranslation");
								DisableNameOfProcessing = reader.GetAttribute("DisableNameOfProcessing");
								
								EnableModules = reader.GetAttribute("EnableModules");

								CustomPathToDLLs = reader.GetAttribute("CustomPathToDLLs");
								CustomCSNamesToJS = reader.GetAttribute("CustomCSNamesToJS");
								AddSBAtTheTop = reader.GetAttribute("AddSBAtTheTop");
								AddSBAtTheBottom = reader.GetAttribute("AddSBAtTheBottom");

								if (Debug != null)
								{
									if (currentFile == null)
										defaultOptions.Debug = bool.Parse(Debug);
									else
										currentFile.OptionsForFile.Debug = bool.Parse(Debug);
									break;
								}
								if (DisableDiagnostics != null)
								{
									if (currentFile == null)
										defaultOptions.DisableDiagnostics = bool.Parse(DisableDiagnostics);
									else
										currentFile.OptionsForFile.DisableDiagnostics = bool.Parse(DisableDiagnostics);
									break;
								}
								if (UseVarOverLet != null)
								{
									if (currentFile == null)
										defaultOptions.UseVarOverLet = bool.Parse(UseVarOverLet);
									else
										currentFile.OptionsForFile.UseVarOverLet = bool.Parse(UseVarOverLet);
									break;
								}
								if (KeepBraceOnTheSameLine != null)
								{
									if (currentFile == null)
										defaultOptions.KeepBraceOnTheSameLine = bool.Parse(KeepBraceOnTheSameLine);
									else
										currentFile.OptionsForFile.KeepBraceOnTheSameLine = bool.Parse(KeepBraceOnTheSameLine);
									break;
								}
								if (NormalizeWhitespace != null)
								{
									if (currentFile == null)
										defaultOptions.NormalizeWhitespace = bool.Parse(NormalizeWhitespace);
									else
										currentFile.OptionsForFile.NormalizeWhitespace = bool.Parse(NormalizeWhitespace);
									break;
								}
								if (MakePropertiesEnumerable != null)
								{
									if (currentFile == null)
										defaultOptions.MakePropertiesEnumerable = bool.Parse(MakePropertiesEnumerable);
									else
										currentFile.OptionsForFile.MakePropertiesEnumerable = bool.Parse(MakePropertiesEnumerable);
									break;
								}
								if (DisableTranslation != null)
								{
									if (currentFile == null)
										defaultOptions.DisableTranslation = bool.Parse(DisableTranslation);
									else
										currentFile.OptionsForFile.DisableTranslation = bool.Parse(DisableTranslation);
									break;
								}
								if (DisableNameOfProcessing != null)
								{
									if (currentFile == null)
										defaultOptions.DisableNameOfProcessing = bool.Parse(DisableNameOfProcessing);
									else
										currentFile.OptionsForFile.DisableNameOfProcessing = bool.Parse(DisableNameOfProcessing);
									break;
								}
								
								if (EnableModules != null)
								{
									if (currentFile == null)
										defaultOptions.EnableModules = byte.Parse(EnableModules);
									else
										currentFile.OptionsForFile.EnableModules = byte.Parse(EnableModules);
									break;
								}

								if (CustomPathToDLLs != null)
								{
									if (currentFile == null)
										defaultOptions.CustomPathToDLLs = CustomPathToDLLs;
									else
										currentFile.OptionsForFile.CustomPathToDLLs = CustomPathToDLLs;
									break;
								}
								if (CustomCSNamesToJS != null)
								{
									Dictionary<string, string> _customCSNamesToJSList = new();
									string[] _localTuples;

									if (CustomCSNamesToJS.Contains(','))
									{
										_localTuples = CustomCSNamesToJS.Split(",");

										for (int i = 0; i < _localTuples.Length; i++)
										{
											string[] _local = _localTuples[i].Split("-");
											_customCSNamesToJSList.Add(_local[0], _local[1]);
										}
									}
									else
									{
										if (CustomCSNamesToJS.Contains('-'))
										{
											string[] _local = CustomCSNamesToJS.Split("-");
											_customCSNamesToJSList.Add(_local[0], _local[1]);
										}
									}

									if (currentFile == null)
										defaultOptions.CustomCSNamesToJS = _customCSNamesToJSList;
									else
										currentFile.OptionsForFile.CustomCSNamesToJS = _customCSNamesToJSList;
									break;
								}
								if (AddSBAtTheTop != null)
								{
									StringBuilder _sb = new();
									_sb.Append(AddSBAtTheTop);

									if (currentFile == null)
										defaultOptions.AddSBAtTheTop = _sb;
									else
										currentFile.OptionsForFile.AddSBAtTheTop = _sb;
									break;
								}
								if (AddSBAtTheBottom != null)
								{
									StringBuilder _sb = new();
									_sb.Append(AddSBAtTheBottom);

									if (currentFile == null)
										defaultOptions.AddSBAtTheBottom = _sb;
									else
										currentFile.OptionsForFile.AddSBAtTheBottom = _sb;
									break;
								}

								//https://stackoverflow.com/a/21009476
								if (reader.MoveToNextAttribute())
								{
									string _name = reader.Name;
									string _value = reader.Value;

									Log.ErrorLine($"Unknown attribute! Attribute name: '{_name}' Attribute value: '{_value}'");
								}
								data.Error = true;
								return data;
							}
							break;
						}
					case XmlNodeType.EndElement:
						{
							//Log.WriteLine($"End Element {reader.Name}");

							if (reader.Name == "File")
								currentFile = null;
							break;
						}
					default:
						//Log.WriteLine($"-Other node: {reader.NodeType} | Value: {reader.Value}");
						break;
				}
			}
		}

		return data;
	}

	public static async Task TranslateAction(ParseResult result)
	{
		SetupLog(result);
		
		XMLData data = ReadXML(result);
		if (data.Error)
			return;
		if (data.Files.Count == 0)
		{
			string projectPath = result.GetValue<string>("--project") ?? "./cstojs_options.xml";
			Log.ErrorLine($"No files specified in the '{projectPath}'.");
			return;
		}

		FileData2[] translatedFiles = (FileData2[])CSTOJS.Translate(data.Files.ToArray());

		for (int i = 0; i < translatedFiles.Length; i++)
		{
			string pathCombined = Path.Combine(data.OutputPath, translatedFiles[i].FileName);
			await File.WriteAllTextAsync(pathCombined, translatedFiles[i].TranslatedStr);

			if (translatedFiles[i].OptionsForFile.Debug)
			{
				for (int j = 0; j < translatedFiles[i].DebugStrings.Length; j++)
				{
					await File.WriteAllTextAsync(pathCombined + $".{j}.cstojs", translatedFiles[i].DebugStrings[j]);
				}
			}
		}

		Log.InfoLine($"--- Directory: {Path.GetFullPath(data.OutputPath)}");
		for (int i = 0; i < translatedFiles.Length; i++)
		{
			Log.InfoLine($"--- --- File: {translatedFiles[i].FileName}");
		}
	}
	public static async Task WatchAction(ParseResult result)
	{
		SetupLog(result);

		XMLData data = ReadXML(result);
		if (data.Error)
			return;
		if (data.Files.Count == 0)
		{
			string projectPath = result.GetValue<string>("--project") ?? "./cstojs_options.xml";
			Log.ErrorLine($"No files specified in the '{projectPath}'.");
			return;
		}

		Log.InfoLine("Press ctrl+c to end watching.");
		Console.CancelKeyPress += Exit;

		Dictionary<string, DateTime> dateTimes = new();
		for (int i = 0; i < data.Files.Count; i++)
		{
			dateTimes.Add(data.Files[i].SourceFilePath, DateTime.UtcNow);
		}

		int delay = result.GetValue<int>("--delay");

		while (RunWatch)
		{
			for (int i = 0; i < data.Files.Count; i++)
			{
				DateTime time = File.GetLastWriteTimeUtc(data.Files[i].SourceFilePath);
				if (time > dateTimes[data.Files[i].SourceFilePath])
				{
					try
					{
						data.Files[i].SourceStr = File.ReadAllText(data.Files[i].SourceFilePath);

						FileData2 translatedFile = (FileData2)CSTOJS.Translate(data.Files[i]);

						string pathCombined = Path.Combine(data.OutputPath, translatedFile.FileName);
						await File.WriteAllTextAsync(pathCombined, translatedFile.TranslatedStr);

						if (translatedFile.OptionsForFile.Debug)
						{
							for (int j = 0; j < translatedFile.DebugStrings.Length; j++)
							{
								await File.WriteAllTextAsync(pathCombined + $".{j}.cstojs", translatedFile.DebugStrings[j]);
							}
						}

						Log.InfoLine($"--- Directory: {Path.GetFullPath(data.OutputPath)}");
						Log.InfoLine($"--- --- File: {translatedFile.FileName}");

						dateTimes[data.Files[i].SourceFilePath] = time;
					}
					catch (Exception e)
					{
						RunWatch = false;
						Log.ErrorLine(e.ToString());
						break;
					}
				}
			}

			//Todo? Timer rather than thread sleep?
			Thread.Sleep(delay);
		}
	}
	public static async Task OptionAction(ParseResult result)
	{
		SetupLog(result);

		string projectPath = result.GetValue<string>("--project") ?? "./cstojs_options.xml";

		string fileSourceOption = result.GetValue<string>("--file") ?? string.Empty;

		//
		bool? Debug = result.GetValue<bool?>("--Debug") ?? null;
		bool? DisableDiagnostics = result.GetValue<bool?>("--DisableDiagnostics") ?? null;
		bool? UseVarOverLet = result.GetValue<bool?>("--UseVarOverLet") ?? null;
		bool? KeepBraceOnTheSameLine = result.GetValue<bool?>("--KeepBraceOnTheSameLine") ?? null;
		bool? NormalizeWhitespace = result.GetValue<bool?>("--NormalizeWhitespace") ?? null;
		bool? MakePropertiesEnumerable = result.GetValue<bool?>("--MakePropertiesEnumerable") ?? null;
		bool? DisableTranslation = result.GetValue<bool?>("--DisableTranslation") ?? null;
		bool? DisableNameOfProcessing = result.GetValue<bool?>("--DisableNameOfProcessing") ?? null;

		byte? EnableModules = result.GetValue<byte?>("--EnableModules") ?? null;

		string? CustomPathToDLLs = result.GetValue<string>("--CustomPathToDLLs") ?? null;
		string? CustomCSNamesToJS = result.GetValue<string>("--CustomCSNamesToJS") ?? null;
		string? AddSBAtTheTop = result.GetValue<string>("--AddSBAtTheTop") ?? null;
		string? AddSBAtTheBottom = result.GetValue<string>("--AddSBAtTheBottom") ?? null;
		

		List<string> optionNames = new();
		if (Debug != null)
			optionNames.Add(nameof(Debug));
		if (DisableDiagnostics != null)
			optionNames.Add(nameof(DisableDiagnostics));
		if (UseVarOverLet != null)
			optionNames.Add(nameof(UseVarOverLet));
		if (KeepBraceOnTheSameLine != null)
			optionNames.Add(nameof(KeepBraceOnTheSameLine));
		if (NormalizeWhitespace != null)
			optionNames.Add(nameof(NormalizeWhitespace));
		if (MakePropertiesEnumerable != null)
			optionNames.Add(nameof(MakePropertiesEnumerable));
		if (DisableTranslation != null)
			optionNames.Add(nameof(DisableTranslation));
		if (DisableNameOfProcessing != null)
			optionNames.Add(nameof(DisableNameOfProcessing));

		if (EnableModules != null)
			optionNames.Add(nameof(EnableModules));
			
		if (CustomPathToDLLs != null)
			optionNames.Add(nameof(CustomPathToDLLs));
		if (CustomCSNamesToJS != null)
			optionNames.Add(nameof(CustomCSNamesToJS));
		if (AddSBAtTheTop != null)
			optionNames.Add(nameof(AddSBAtTheTop));
		if (AddSBAtTheBottom != null)
			optionNames.Add(nameof(AddSBAtTheBottom));
		//
		
		bool removeOption = result.GetValue<bool>("--remove");
		bool addOption = result.GetValue<bool>("--add");
		string changeOption = result.GetValue<string>("--change") ?? string.Empty;

		if (!removeOption && !addOption && changeOption == string.Empty)
		{
			Log.ErrorLine("--remove/--add/--change options must be specified!");
			return;
		}

		XmlDocument doc = new();
		doc.Load(projectPath);
		XmlElement? root = doc.DocumentElement;
		if (root == null || root.Name != "ProjectOptions")
		{
			Log.ErrorLine("root is null or root is not 'ProjectOptions'. You can create a project file with the init command.");
			return;
		}

		if (removeOption)
		{
			Log.InfoLine("'--remove' option is used.");

			XmlNodeList _childNodes = root.ChildNodes;
			if (fileSourceOption != string.Empty)
			{
				for (int i = 0; i <= _childNodes.Count; i++)
				{
					if (i == _childNodes.Count)
					{
						Log.WarningLine($"Cannot locate a File element with the source: '{fileSourceOption}'.");
						return;
					}
					if (_childNodes[i].Attributes[0].Value == fileSourceOption)
					{
						root = (XmlElement)_childNodes[i];
						_childNodes = _childNodes[i].ChildNodes;
						break;
					}
				}
				Log.InfoLine($"Under a File element: '{fileSourceOption}'");
			}

			for (int i = 0; i < optionNames.Count; i++)
			{
				for (int j = 0; j <= _childNodes.Count; j++)
				{
					if (j == _childNodes.Count)
					{
						Log.WarningLine($"Cannot locate an Option element with an attribute name: '{optionNames[i]}'. Is it already removed?");
						break;
					}
					if (_childNodes[j].Attributes[0].Name == optionNames[i])
					{
						Log.InfoLine($"Removing an Option element with an attribute name: '{optionNames[i]}'. Child id: {j}.");
						root.RemoveChild(_childNodes[j]);
						break;
					}
				}
			}
		}

		if (addOption)
		{
			Log.InfoLine("'--add' option is used.");
			
			XmlNodeList _childNodes = root.ChildNodes;
			if (fileSourceOption != string.Empty)
			{
				for (int i = 0; i <= _childNodes.Count; i++)
				{
					if (i == _childNodes.Count)
					{
						Log.WarningLine($"Cannot locate a File element with the source: '{fileSourceOption}'.");
						return;
					}
					if (_childNodes[i].Attributes[0].Value == fileSourceOption)
					{
						root = (XmlElement)_childNodes[i];
						_childNodes = _childNodes[i].ChildNodes;
						break;
					}
				}
				Log.InfoLine($"Under a File element: '{fileSourceOption}'");
			}
			
			for (int i = 0; i < optionNames.Count; i++)
			{
				for (int j = 0; j <= _childNodes.Count; j++)
				{
					if (j == _childNodes.Count)
					{
						//Log.InfoLine($"Appending an Option element with name: '{optionNames[i]}' and with value: '{result.GetValue($"--{optionNames[i]}")}'.");
						//XmlElement _option = doc.CreateElement("Option");
						//_option.SetAttribute(optionNames[i], fileSource);
						//root.AppendChild(_option);
						break;
					}
					if (_childNodes[j].Attributes[0].Name == optionNames[i])
					{
						Log.WarningLine($"An Option element already exist with an attrinute name: '{optionNames[i]}'. If you want to change a value use '--change'");
						break;
					}
				}
			}
		}
		
		Log.InfoLine($"Saving a project file: '{projectPath}'");
		doc.Save(projectPath);
	}
	public static async Task FileAction(ParseResult result)
	{
		SetupLog(result);

		string projectPath = result.GetValue<string>("--project") ?? "./cstojs_options.xml";

		string fileSource = result.GetRequiredValue<string>("file-source");

		bool removeOption = result.GetValue<bool>("--remove");
		bool addOption = result.GetValue<bool>("--add");
		string changeOption = result.GetValue<string>("--change") ?? string.Empty;

		if (!removeOption && !addOption && changeOption == string.Empty)
		{
			Log.ErrorLine("--remove/--add/--change options must be specified!");
			return;
		}
		
		XmlDocument doc = new();
		doc.Load(projectPath);
		XmlElement? root = doc.DocumentElement;
		if (root == null || root.Name != "ProjectOptions")
		{
			Log.ErrorLine("root is null or root is not 'ProjectOptions'. You can create a project file with the init command.");
			return;
		}
		if (removeOption)
		{
			Log.InfoLine("'--remove' option is used.");
			
			XmlNodeList _childNodes = root.ChildNodes;
			for (int i = 0; i <= _childNodes.Count; i++)
			{
				if (i == _childNodes.Count)
				{
					Log.WarningLine($"Cannot locate a File element with source: '{fileSource}'. Is it already removed?");
					break;
				}
				if (_childNodes[i].Attributes[0].Value == fileSource)
				{
					Log.InfoLine($"Removing a File element with source: '{fileSource}'. Child id: {i}.");
					root.RemoveChild(_childNodes[i]);
					break;
				}
			}
		}

		if (addOption)
		{
			Log.InfoLine("'--add' option is used.");

			if (!File.Exists(fileSource))
			{
				Log.ErrorLine($"File does not exist: '{fileSource}'. Skipping '--add' option.");
			}
			else
			{

				XmlNodeList _childNodes = root.ChildNodes;
				for (int i = 0; i <= _childNodes.Count; i++)
				{
					if (i == _childNodes.Count)
					{
						Log.InfoLine($"Appending to the root a File element with source: '{fileSource}'.");
						XmlElement _file = doc.CreateElement("File");
						_file.SetAttribute("Source", fileSource);
						root.AppendChild(_file);
						break;
					}
					if (_childNodes[i].Attributes[0].Value == fileSource)
					{
						Log.WarningLine($"A File element already exist with source: '{fileSource}'. If you want to change source use '--change'");
						break;
					}
				}
			}
		}

		if (changeOption != string.Empty)
		{
			Log.InfoLine($"'--change' option is used with: '{changeOption}'.");

			if (!File.Exists(changeOption))
			{
				Log.ErrorLine($"File does not exist: '{changeOption}'. Skipping '--change' option.");
			}
			else
			{

				XmlNodeList _childNodes = root.ChildNodes;
				for (int i = 0; i <= _childNodes.Count; i++)
				{
					if (i == _childNodes.Count)
					{
						Log.WarningLine($"Could not find a File element with source: '{fileSource}'.");
						break;
					}
					if (_childNodes[i].Attributes[0].Value == fileSource)
					{
						Log.InfoLine($"Changing a File element with source: '{fileSource}' to '{changeOption}'.");
						_childNodes[i].Attributes[0].Value = changeOption;
						break;
					}
				}
			}
		}
		
		Log.InfoLine($"Saving a project file: '{projectPath}'");
		doc.Save(projectPath);
	}
	private static void SetupLog(ParseResult result)
	{
		Log.DisableConsoleColors = result.GetValue<bool>("--disable-console-colors");
		Log.DisableConsoleOutput = result.GetValue<bool>("--disable-console-output");
	}
	
	private static void Exit(object? sender, ConsoleCancelEventArgs e)
	{
		Log.InfoLine("Exiting.");
		RunWatch = false;
	}
}

public class XMLData
{
	public List<FileData2> Files { get; set; } = new();
	public string OutputPath { get; set; } = string.Empty;
	public bool Error = false;

	public XMLData() { }

}
public class FileData2 : FileData
{
	public string SourceFilePath = string.Empty;
}
