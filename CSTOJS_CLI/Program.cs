using CSharpToJavaScript;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Text;
using CSharpToJavaScript.Utils;

namespace CSTOJS_CLI;

public class Program
{
	private static int Main(string[] args)
	{
		RootCommand rootCommand = new("Dotnet tool/cli for a CSharpToJavaScript library.");

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

		Log.InfoLine("Running: 'dotnet new console -f net10.0'");
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

		Log.InfoLine("Adding CSharpToJavaScript package: 'dotnet add package CSharpToJavaScript'");
		startInfo.Arguments = "add package CSharpToJavaScript";
		proc.Start();
		proc.WaitForExit();

		Log.InfoLine($"Creating an output folder: '{folder}'");
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

	public static async Task TranslateAction(ParseResult result)
	{
		/* TODO?
		Assembly assembly = Assembly.LoadFile("../../CSharpToJavaScript/bin/Debug/net10.0/CSharpToJavaScript.dll");
		var cstojsClass = assembly.GetType("CSharpToJavaScript.CSTOJS");
		var fileData = assembly.GetType("CSharpToJavaScript.FileData");
		
		dynamic fileDataInst = assembly.CreateInstance("CSharpToJavaScript.FileData", false,
			BindingFlags.ExactBinding,
			null, null, null, null);
		fileDataInst.SourceStr = "Console.WriteLine();";

		MethodInfo m = cstojsClass.GetMethod("Translate");
		dynamic ret = m.Invoke(null, new dynamic[] { new dynamic[] { fileDataInst }, null });
		Log.WriteLine($"SampleMethod returned {ret[0].TranslatedStr}.");
		*/
		
		CSTOJSOptions defaultOptions = new();

		FileData? currentFile = null;

		string outputPath = string.Empty;

		List<FileData> files = new();

		string pathCombined = string.Empty;

		//Options:
		string? Debug = null;
		string? UseVarOverLet = null;
		string? KeepBraceOnTheSameLine = null;
		string? NormalizeWhitespace = null;
		string? UseStrictEquality = null;
		string? TranslateFile = null;
		
		string? CustomCSNamesToJS = null;
		string? AddSBAtTheTop = null;
		string? AddSBAtTheBottom = null;

		using (XmlReader reader = XmlReader.Create("cstojs_options.xml"))
		{
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						{
							Log.WriteLine($"Start Element {reader.Name}");

							if (reader.Name == "Output")
							{
								string? _output = reader.GetAttribute("Folder");
								if (_output == null)
								{
									Log.ErrorLine("Folder attribute is null!");
									return;
								}
								if (!Directory.Exists(_output))
								{
									Log.ErrorLine($"Directory does not exists: {_output}");
									return;
								}
								outputPath = _output;
								break;
							}
							if (reader.Name == "File")
							{
								string? _source = reader.GetAttribute("Source");
								if (_source == null)
								{
									Log.ErrorLine("Source attribute is null!");
									return;
								}
								if (!File.Exists(_source))
								{
									Log.ErrorLine($"File does not exists: {_source}");
									return;
								}

								currentFile = new()
								{
									PathID = _source.Replace(".cs", ".js"),
									SourceStr = File.ReadAllText(_source)
								};

								if (reader.IsEmptyElement)
									currentFile.OptionsForFile = defaultOptions;
								else
									currentFile.OptionsForFile = new();

								files.Add(currentFile);
								break;
							}
							if (reader.Name == "Option")
							{
								Debug = reader.GetAttribute("Debug");
								UseVarOverLet = reader.GetAttribute("UseVarOverLet");
								KeepBraceOnTheSameLine = reader.GetAttribute("KeepBraceOnTheSameLine");
								NormalizeWhitespace = reader.GetAttribute("NormalizeWhitespace");
								UseStrictEquality = reader.GetAttribute("UseStrictEquality");
								TranslateFile = reader.GetAttribute("TranslateFile");

								CustomCSNamesToJS = reader.GetAttribute("CustomCSNamesToJS");
								AddSBAtTheTop = reader.GetAttribute("AddSBAtTheTop");
								AddSBAtTheBottom = reader.GetAttribute("AddSBAtTheBottom");

								if (Debug != null)
								{
									if (currentFile == null)
										defaultOptions.Debug = bool.Parse(Debug);
									else
										currentFile.OptionsForFile.Debug = bool.Parse(Debug);
								}
								if (UseVarOverLet != null)
								{
									if (currentFile == null)
										defaultOptions.UseVarOverLet = bool.Parse(UseVarOverLet);
									else
										currentFile.OptionsForFile.UseVarOverLet = bool.Parse(UseVarOverLet);
								}
								if (KeepBraceOnTheSameLine != null)
								{
									if (currentFile == null)
										defaultOptions.KeepBraceOnTheSameLine = bool.Parse(KeepBraceOnTheSameLine);
									else
										currentFile.OptionsForFile.KeepBraceOnTheSameLine = bool.Parse(KeepBraceOnTheSameLine);
								}
								if (NormalizeWhitespace != null)
								{
									if (currentFile == null)
										defaultOptions.NormalizeWhitespace = bool.Parse(NormalizeWhitespace);
									else
										currentFile.OptionsForFile.NormalizeWhitespace = bool.Parse(NormalizeWhitespace);
								}
								if (UseStrictEquality != null)
								{
									if (currentFile == null)
										defaultOptions.UseStrictEquality = bool.Parse(UseStrictEquality);
									else
										currentFile.OptionsForFile.UseStrictEquality = bool.Parse(UseStrictEquality);
								}
								if (TranslateFile != null)
								{
									if (currentFile == null)
										defaultOptions.TranslateFile = bool.Parse(TranslateFile);
									else
										currentFile.OptionsForFile.TranslateFile = bool.Parse(TranslateFile);
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
								}
								if (AddSBAtTheTop != null)
								{
									StringBuilder _sb = new();
									_sb.Append(AddSBAtTheTop);
									
									if (currentFile == null)
										defaultOptions.AddSBAtTheTop = _sb;
									else
										currentFile.OptionsForFile.AddSBAtTheTop = _sb;
								}
								if (AddSBAtTheBottom != null)
								{
									StringBuilder _sb = new();
									_sb.Append(AddSBAtTheBottom);

									if (currentFile == null)
										defaultOptions.AddSBAtTheBottom = _sb;
									else
										currentFile.OptionsForFile.AddSBAtTheBottom = _sb;
								}
								
								Log.ErrorLine($"Unknown option: value: {reader.GetAttribute(0)}");
								break;
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

		FileData[] translatedFiles = CSTOJS.Translate(files.ToArray());

		for (int i = 0; i < translatedFiles.Length; i++)
		{
			pathCombined = Path.Combine(outputPath, translatedFiles[i].PathID);
			await File.WriteAllTextAsync(pathCombined, translatedFiles[i].TranslatedStr);
		}
		
		Log.InfoLine($"--- Done: {Path.GetFullPath(outputPath)}");
	}
}