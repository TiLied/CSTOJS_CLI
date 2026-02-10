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
	private static int Main(string[] args)
	{
		RootCommand rootCommand = new("Dotnet tool/cli for a CSharpToJavaScript library.");

		Argument<string> outputArgument = new("folder")
		{
			Description = "Output folder. Can be absolute path or relative."
		};

		Command initCommand = new("init", "Create a barebone 'cstojs_options.xml', without running the dotnet commands.");
		initCommand.Arguments.Add(outputArgument);
		initCommand.SetAction(InitAction);

		Command setupCommand = new("setup", "Setup cstojs project.");
		setupCommand.Arguments.Add(outputArgument);
		setupCommand.SetAction(SetupAction);

		Option<string> projectPath = new("--project", "-p")
		{
			HelpName = "path",
			Description = "Path to the 'cstojs_options.xml'.",
			DefaultValueFactory = (r) => { return "./cstojs_options.xml"; }
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
		
		Command watchCommand = new("watch", "Watches specified files in the 'cstojs_options.xml' with an interval and translates them. Note: The 'cstojs_options.xml' file is not being monitored, so any changes require the command to be restarted.");
		watchCommand.SetAction(WatchAction);
		watchCommand.Options.Add(projectPath);
		watchCommand.Options.Add(delayWatch);
		
		rootCommand.Subcommands.Add(initCommand);
		rootCommand.Subcommands.Add(setupCommand);
		rootCommand.Subcommands.Add(translateCommand);
		rootCommand.Subcommands.Add(watchCommand);

		ParseResult parseResult = rootCommand.Parse(args);
		return parseResult.Invoke();
	}
	public static void InitAction(ParseResult result)
	{
		if (File.Exists("./cstojs_options.xml"))
		{
			Log.ErrorLine($"'cstojs_options.xml' already exists!");
			return;
		}

		string folder = result.GetRequiredValue<string>("folder");

		Log.InfoLine($"Creating an output folder: '{Path.GetFullPath(folder)}'");
		Directory.CreateDirectory(folder);

		Log.InfoLine($"Creating 'cstojs_options.xml'");
		XmlDocument doc = new();

		XmlElement root = doc.CreateElement(string.Empty, "ProjectOptions", string.Empty);

		XmlElement output = doc.CreateElement("Output");
		output.SetAttribute("Folder", folder);
		root.AppendChild(output);

		doc.AppendChild(root);
		doc.Save("cstojs_options.xml");

		Log.InfoLine($"Init ended!");
	}
	public static void SetupAction(ParseResult result)
	{
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

		Log.InfoLine($"Creating an output folder: '{Path.GetFullPath(folder)}'");
		Directory.CreateDirectory(folder);

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
		if (!File.Exists(projectPath))
		{
			Log.ErrorLine($"File 'cstojs_options.xml' does not exists: {projectPath}");
			data.Error = true;
			return data;
		}
		string directoryPath = Path.GetDirectoryName(Path.GetFullPath(projectPath)) ?? "./";
		CSTOJSOptions defaultOptions = new();

		FileData2? currentFile = null;

		string pathCombined = string.Empty;

		//CSTOJS Options:
		string? Debug = null;
		string? DisableCompilationErrors = null;
		string? UseVarOverLet = null;
		string? KeepBraceOnTheSameLine = null;
		string? NormalizeWhitespace = null;
		string? TranslateFile = null;
		string? MakePropertiesEnumerable = null;

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
									JSFileName = _sourceFileName.Replace(".cs", ".js"),
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
								DisableCompilationErrors = reader.GetAttribute("DisableCompilationErrors");
								UseVarOverLet = reader.GetAttribute("UseVarOverLet");
								KeepBraceOnTheSameLine = reader.GetAttribute("KeepBraceOnTheSameLine");
								NormalizeWhitespace = reader.GetAttribute("NormalizeWhitespace");
								TranslateFile = reader.GetAttribute("TranslateFile");
								MakePropertiesEnumerable = reader.GetAttribute("MakePropertiesEnumerable");

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
								if (DisableCompilationErrors != null)
								{
									if (currentFile == null)
										defaultOptions.DisableCompilationErrors = bool.Parse(DisableCompilationErrors);
									else
										currentFile.OptionsForFile.DisableCompilationErrors = bool.Parse(DisableCompilationErrors);
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
								if (TranslateFile != null)
								{
									if (currentFile == null)
										defaultOptions.TranslateFile = bool.Parse(TranslateFile);
									else
										currentFile.OptionsForFile.TranslateFile = bool.Parse(TranslateFile);
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
		XMLData data = ReadXML(result);
		if (data.Error)
			return;
		if (data.Files.Count == 0)
		{
			Log.ErrorLine("No files specified in 'cstojs_options.xml'.");
			return;
		}

		FileData2[] translatedFiles = (FileData2[])CSTOJS.Translate(data.Files.ToArray());

		for (int i = 0; i < translatedFiles.Length; i++)
		{
			string pathCombined = Path.Combine(data.OutputPath, translatedFiles[i].JSFileName);
			await File.WriteAllTextAsync(pathCombined, translatedFiles[i].TranslatedStr);
		}

		Log.InfoLine($"--- Directory: {Path.GetFullPath(data.OutputPath)}");
		for (int i = 0; i < translatedFiles.Length; i++)
		{
			Log.InfoLine($"--- --- File: {translatedFiles[i].JSFileName}");
		}
	}
	public static bool RunWatch = true;
	public static async Task WatchAction(ParseResult result)
	{
		XMLData data = ReadXML(result);
		if (data.Error)
			return;
		if (data.Files.Count == 0)
		{
			Log.ErrorLine("No files specified in 'cstojs_options.xml'.");
			return;
		}

		Log.InfoLine("Press ctrl+c to end watching.");

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

						string pathCombined = Path.Combine(data.OutputPath, translatedFile.JSFileName);
						await File.WriteAllTextAsync(pathCombined, translatedFile.TranslatedStr);

						Log.InfoLine($"--- Directory: {Path.GetFullPath(data.OutputPath)}");
						Log.InfoLine($"--- --- File: {translatedFile.JSFileName}");

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
			
			Thread.Sleep(delay);
		}
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
	public string JSFileName = string.Empty;
	public string SourceFilePath = string.Empty;
}