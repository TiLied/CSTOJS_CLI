using CSharpToJavaScript;
using System.Collections.Generic;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace CSTOJS_CLI
{
	public class Program
	{
		private static async Task<int> Main(string[] args)
		{
			RootCommand rootCommand = new("CLI for CSharpToJavaScript library.");

			Command fileCommand = new("file", "Choose a cs file or the folder with cs files.");
			fileCommand.AddAlias("f");

			Command continuousCommand = new("continuous", "Choose a cs file and generate continuously by watching the cs file.");
			continuousCommand.AddAlias("c");


			//Options CSharpToJavaScript
			Option<bool> debugOption = new(
				new string[] {
					"-Debug",
					"/Debug"},
				"Debug. When set to true prints additional info to console, cs lines to js file.");
			debugOption.SetDefaultValue(false);

			fileCommand.AddOption(debugOption);
			continuousCommand.AddOption(debugOption);

			Option<bool> disableConsoleColorsOption = new(
				new string[] {
					"-DisableConsoleColors",
					"/DisableConsoleColors"},
				"Self-explanatory, Disable Console Colors.");
			disableConsoleColorsOption.SetDefaultValue(false);

			fileCommand.AddOption(disableConsoleColorsOption);
			continuousCommand.AddOption(disableConsoleColorsOption);

			Option<bool> disableConsoleOutputOption = new(
				new string[] {
					"-DisableConsoleOutput",
					"/DisableConsoleOutput"},
				"Self-explanatory, Disable Console Output.");
			disableConsoleOutputOption.SetDefaultValue(false);

			fileCommand.AddOption(disableConsoleOutputOption);
			continuousCommand.AddOption(disableConsoleOutputOption);


			Option<string> outPutPathOption = new(
				new string[] {
					"-OutPutPath",
					"/OutPutPath"},
				"Output path for javascript file.");
			outPutPathOption.SetDefaultValue(Directory.GetCurrentDirectory());

			fileCommand.AddOption(outPutPathOption);
			continuousCommand.AddOption(outPutPathOption);


			Option<bool> useVarOverLetOption = new(
				new string[] {
					"-UseVarOverLet",
					"/UseVarOverLet"},
				"Self-explanatory, Use var over let.");
			useVarOverLetOption.SetDefaultValue(false);

			fileCommand.AddOption(useVarOverLetOption);
			continuousCommand.AddOption(useVarOverLetOption);


			Option<bool> keepBraceOnTheSameLine = new(
				new string[] {
					"-KeepBraceOnTheSameLine",
					"/KeepBraceOnTheSameLine"},
				"Keep Brace '{' on the same line.");
			keepBraceOnTheSameLine.SetDefaultValue(false);

			fileCommand.AddOption(keepBraceOnTheSameLine);
			continuousCommand.AddOption(keepBraceOnTheSameLine);


			Option<bool> normalizeWhitespace = new(
				new string[] {
					"-NormalizeWhitespace",
					"/NormalizeWhitespace"},
				"Self-explanatory, Normalize Whitespace.");
			normalizeWhitespace.SetDefaultValue(false);

			fileCommand.AddOption(normalizeWhitespace);
			continuousCommand.AddOption(normalizeWhitespace);

			Option<bool> useStrictEquality = new(
				new string[] {
								"-UseStrictEquality",
								"/UseStrictEquality"},
				"Replace '==' with '===' and '!=' with '!=='.");
			useStrictEquality.SetDefaultValue(false);

			fileCommand.AddOption(useStrictEquality);
			continuousCommand.AddOption(useStrictEquality);


			Option<string> customCSTypesToJS = new(
				new string[] {
					"-CustomCSNamesToJS",
					"/CustomCSNamesToJS"},
				"List of custom names to convert. Example: Console-console,WriteLine-log");
			customCSTypesToJS.SetDefaultValue(string.Empty);

			fileCommand.AddOption(customCSTypesToJS);
			continuousCommand.AddOption(customCSTypesToJS);


			Argument<string> pathArgument = new("path", "Full path.");
			fileCommand.AddArgument(pathArgument);
			continuousCommand.AddArgument(pathArgument);

			rootCommand.AddCommand(fileCommand);
			rootCommand.AddCommand(continuousCommand);


			CLICSTOJSOptionsBinder binder = new(debugOption,
				disableConsoleColorsOption,
				disableConsoleOutputOption,
				outPutPathOption,
				useVarOverLetOption,
				keepBraceOnTheSameLine,
				normalizeWhitespace,
				useStrictEquality,
				customCSTypesToJS
				);


			fileCommand.SetHandler(GenerateFile, pathArgument, binder);

			continuousCommand.SetHandler(GenerateContinuously, pathArgument, binder);

			return await rootCommand.InvokeAsync(args);
		}

		public static void GenerateContinuously(string? file, CLICSTOJSOptions options) 
		{
			if (file == null)
				return;

			CSTOJS _CSTOJS = InitiateCSTOJS(options);

			_CSTOJS.GenerateOneContinuously(file);

			Console.WriteLine("Press any button to stop wathing.");
			Console.ReadLine();

			_CSTOJS.StopWatching();
		}

		public static async Task GenerateFile(string? file, CLICSTOJSOptions options)
		{
			if (file == null)
				return;

			CSTOJS _CSTOJS = InitiateCSTOJS(options);

			await _CSTOJS.GenerateOneAsync(file);
		}

		private static CSTOJS InitiateCSTOJS(CLICSTOJSOptions options)
		{
			CSTOJS? _CSTOJS;

			if (!options.IsDefault())
			{
				string customNames = options.CustomCSTypesToJS.Replace(" ", "").Trim();

				List<Tuple<string, string>> _cstojsList = new();

				if (customNames != string.Empty)
				{
					List<string> _localTuples = new();
					if (customNames.Contains(','))
					{
						_localTuples = customNames.Split(',').ToList();

						for (int i = 0; i < _localTuples.Count; i++)
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

		public string CustomCSTypesToJS { get; set; } = string.Empty;

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

			if (CustomCSTypesToJS != string.Empty)
				return false;

			return true;
		}
	}

	public class CLICSTOJSOptionsBinder : BinderBase<CLICSTOJSOptions>
	{
		private readonly Option<bool> _DebugOption;
		private readonly Option<bool> _DisableConsoleColorsOption;
		private readonly Option<bool> _DisableConsoleOutputOption;
		private readonly Option<string> _OutPutPathOptionOption;
		private readonly Option<bool> _UseVarOverLetOption;
		private readonly Option<bool> _KeepBraceOnTheSameLine;
		private readonly Option<bool> _NormalizeWhitespace;
		private readonly Option<bool> _UseStrictEquality;

		private readonly Option<string> _CustomCSTypesToJS;
		public CLICSTOJSOptionsBinder(Option<bool> debugOption,
			Option<bool> disableConsoleColorsOption,
			Option<bool> disableConsoleOutputOption,
			Option<string> outPutPathOptionOption,
			Option<bool> useVarOverLetOption,
			Option<bool> keepBraceOnTheSameLine,
			Option<bool> normalizeWhitespace,
			Option<bool> useStrictEquality,
			Option<string> customCSTypesToJS)
		{
			_DebugOption = debugOption;
			_DisableConsoleColorsOption = disableConsoleColorsOption;
			_DisableConsoleOutputOption = disableConsoleOutputOption;
			_OutPutPathOptionOption = outPutPathOptionOption;
			_UseVarOverLetOption = useVarOverLetOption;
			_KeepBraceOnTheSameLine = keepBraceOnTheSameLine;
			_NormalizeWhitespace = normalizeWhitespace;
			_UseStrictEquality = useStrictEquality;
			_CustomCSTypesToJS = customCSTypesToJS;
		}

		protected override CLICSTOJSOptions GetBoundValue(BindingContext bindingContext)
		{ 
			return new()
			{
				Debug = bindingContext.ParseResult.GetValueForOption(_DebugOption),
				DisableConsoleColors = bindingContext.ParseResult.GetValueForOption(_DisableConsoleColorsOption),
				DisableConsoleOutput = bindingContext.ParseResult.GetValueForOption(_DisableConsoleOutputOption),
				OutPutPath = bindingContext.ParseResult.GetValueForOption(_OutPutPathOptionOption),
				UseVarOverLet = bindingContext.ParseResult.GetValueForOption(_UseVarOverLetOption),
				KeepBraceOnTheSameLine = bindingContext.ParseResult.GetValueForOption(_KeepBraceOnTheSameLine),
				NormalizeWhitespace = bindingContext.ParseResult.GetValueForOption(_NormalizeWhitespace),
				UseStrictEquality = bindingContext.ParseResult.GetValueForOption(_UseStrictEquality),
				CustomCSTypesToJS = bindingContext.ParseResult.GetValueForOption(_CustomCSTypesToJS)
			};
		}
	}
}