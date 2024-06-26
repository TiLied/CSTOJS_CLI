﻿using CSharpToJavaScript;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Threading.Tasks;

namespace CSTOJS_CLI
{
	public class Program
	{
		private static async Task<int> Main(string[] args)
		{
			RootCommand rootCommand = new("CLI for CSharpToJavaScript library.");

			Command fileCommand = new("file", "Choose a cs file or the folder with cs files.");
			fileCommand.AddAlias("f");




			//Options CSharpToJavaScript
			Option<bool> debugOption = new(
				new string[] {
					"-Debug",
					"/Debug"},
				"Debug. When set to true prints additional info to console, cs lines to js file.");
			debugOption.SetDefaultValue(false);

			fileCommand.AddOption(debugOption);


			Option<bool> disableConsoleColorsOption = new(
				new string[] {
					"-DisableConsoleColors",
					"/DisableConsoleColors"},
				"Self-explanatory, Disable Console Colors.");
			disableConsoleColorsOption.SetDefaultValue(false);

			fileCommand.AddOption(disableConsoleColorsOption);


			Option<bool> disableConsoleOutputOption = new(
				new string[] {
					"-DisableConsoleOutput",
					"/DisableConsoleOutput"},
				"Self-explanatory, Disable Console Output.");
			disableConsoleOutputOption.SetDefaultValue(false);

			fileCommand.AddOption(disableConsoleOutputOption);



			Option<string> outPutPathOption = new(
				new string[] {
					"-OutPutPath", 
					"/OutPutPath"},
				"Output path for javascript file.");
			outPutPathOption.SetDefaultValue(Directory.GetCurrentDirectory());

			fileCommand.AddOption(outPutPathOption);



			Option<bool> useVarOverLetOption = new(
				new string[] {
					"-UseVarOverLet",
					"/UseVarOverLet"},
				"Self-explanatory, Use var over let.");
			useVarOverLetOption.SetDefaultValue(false);

			fileCommand.AddOption(useVarOverLetOption);



			Option<bool> keepBraceOnTheSameLine = new(
				new string[] {
					"-KeepBraceOnTheSameLine",
					"/KeepBraceOnTheSameLine"},
				"Keep Brace { on the same line.");
			keepBraceOnTheSameLine.SetDefaultValue(false);

			fileCommand.AddOption(keepBraceOnTheSameLine);



			Option<bool> normalizeWhitespace = new(
				new string[] {
					"-NormalizeWhitespace",
					"/NormalizeWhitespace"},
				"Self-explanatory, Normalize Whitespace.");
			normalizeWhitespace.SetDefaultValue(false);

			fileCommand.AddOption(normalizeWhitespace);


			Argument<string> pathArgument = new("path", "Full path.");
			fileCommand.AddArgument(pathArgument);

			rootCommand.AddCommand(fileCommand);


			CLICSTOJSOptionsBinder binder = new(debugOption,
				disableConsoleColorsOption,
				disableConsoleOutputOption,
				outPutPathOption,
				useVarOverLetOption,
				keepBraceOnTheSameLine,
				normalizeWhitespace);


			fileCommand.SetHandler(GenerateFile, pathArgument, binder);

			return await rootCommand.InvokeAsync(args);
		}

		public static async Task GenerateFile(string? file, CLICSTOJSOptions options)
		{
			if (file == null)
				return;

			CSTOJS? _CSTOJS;

			if (!options.IsDefault())
			{
				_CSTOJS = new(new()
				{
					Debug = options.Debug,
					DisableConsoleColors = options.DisableConsoleColors,
					OutPutPath = options.OutPutPath,
					UseVarOverLet = options.UseVarOverLet,
					KeepBraceOnTheSameLine = options.KeepBraceOnTheSameLine,
					NormalizeWhitespace = options.NormalizeWhitespace
				});
			}
			else
				_CSTOJS = new();

			await _CSTOJS.GenerateOneAsync(file);
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
		//public List<Tuple<string, string>> CustomCSNamesToJS { get; set; } = new();
		//public List<Type> CustomCSTypesToJS { get; set; } = new();
		//public StringBuilder AddSBInFront { get; set; } = new();
		//public StringBuilder AddSBInEnd { get; set; } = new();

		public bool IsDefault() 
		{
			if (Debug != false ||
				DisableConsoleColors != false ||
				DisableConsoleOutput != false ||
				UseVarOverLet != false ||
				KeepBraceOnTheSameLine != false ||
				NormalizeWhitespace != false)
			{
				return false;
			}

			if(OutPutPath != Directory.GetCurrentDirectory())
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

		public CLICSTOJSOptionsBinder(Option<bool> debugOption, 
			Option<bool> disableConsoleColorsOption,
			Option<bool> disableConsoleOutputOption,
			Option<string> outPutPathOptionOption, 
			Option<bool> UseVarOverLetOption,
			Option<bool> KeepBraceOnTheSameLine,
			Option<bool> NormalizeWhitespace)
		{
			_DebugOption = debugOption;
			_DisableConsoleColorsOption = disableConsoleColorsOption;
			_DisableConsoleOutputOption = disableConsoleOutputOption;
			_OutPutPathOptionOption = outPutPathOptionOption;
			_UseVarOverLetOption = UseVarOverLetOption;
			_KeepBraceOnTheSameLine = KeepBraceOnTheSameLine;
			_NormalizeWhitespace = NormalizeWhitespace;
		}

		protected override CLICSTOJSOptions GetBoundValue(BindingContext bindingContext) =>
			new()
			{
				Debug = bindingContext.ParseResult.GetValueForOption(_DebugOption),
				DisableConsoleColors = bindingContext.ParseResult.GetValueForOption(_DisableConsoleColorsOption),
				DisableConsoleOutput = bindingContext.ParseResult.GetValueForOption(_DisableConsoleOutputOption),
				OutPutPath = bindingContext.ParseResult.GetValueForOption(_OutPutPathOptionOption),
				UseVarOverLet = bindingContext.ParseResult.GetValueForOption(_UseVarOverLetOption),
				KeepBraceOnTheSameLine = bindingContext.ParseResult.GetValueForOption(_KeepBraceOnTheSameLine),
				NormalizeWhitespace = bindingContext.ParseResult.GetValueForOption(_NormalizeWhitespace)
			};
	}
}