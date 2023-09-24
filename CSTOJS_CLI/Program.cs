using CSharpToJavaScript;
using System.CommandLine;
using System.CommandLine.Binding;

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
				"Debug. When set to true prints additional info to console, cs lines to js file and creates file Debug.txt.");
			debugOption.SetDefaultValue(false);

			fileCommand.AddOption(debugOption);

			Option<bool> disableConsoleColorsOption = new(
				new string[] {
					"-DisableConsoleColors",
					"/DisableConsoleColors"},
				"Self-explanatory, Disable Console Colors.");
			disableConsoleColorsOption.SetDefaultValue(false);

			fileCommand.AddOption(disableConsoleColorsOption);

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



			Argument<string> pathArgument = new("path", "Full path.");
			fileCommand.AddArgument(pathArgument);

			rootCommand.AddCommand(fileCommand);

			fileCommand.SetHandler(GenerateFile, pathArgument, new CLICSTOJSOptionsBinder(debugOption, disableConsoleColorsOption, outPutPathOption, useVarOverLetOption));

			return await rootCommand.InvokeAsync(args);
		}

		public static async Task GenerateFile(string? file, CLICSTOJSOptions? options)
		{
			if (file == null)
				return;
		
			CSTOJS? _CSTOJS = null;

			if (!options.IsDefault())
			{
				_CSTOJS = new(new()
				{
					Debug = options.Debug,
					DisableConsoleColors = options.DisableConsoleColors,
					OutPutPath = options.OutPutPath,
					UseVarOverLet = options.UseVarOverLet
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
		public string OutPutPath { get; set; } = Directory.GetCurrentDirectory();
		public bool UseVarOverLet { get; set; } = false;
		//public List<Tuple<string, string>> CustomCSNamesToJS { get; set; } = new();
		//public List<Type> CustomCSTypesToJS { get; set; } = new();
		//public StringBuilder AddSBInFront { get; set; } = new();
		//public StringBuilder AddSBInEnd { get; set; } = new();

		public bool IsDefault() 
		{
			if(Debug != false || DisableConsoleColors != false || UseVarOverLet != false)
				return false;

			if(OutPutPath != Directory.GetCurrentDirectory())
				return false;

			return true;
		}
	}

	public class CLICSTOJSOptionsBinder : BinderBase<CLICSTOJSOptions>
	{
		private readonly Option<bool> _DebugOption;
		private readonly Option<bool> _DisableConsoleColorsOption;
		private readonly Option<string> _OutPutPathOptionOption;
		private readonly Option<bool> _UseVarOverLetOption;

		public CLICSTOJSOptionsBinder(Option<bool> debugOption, Option<bool> disableConsoleColorsOption, Option<string?> outPutPathOptionOption, Option<bool> UseVarOverLetOption)
		{
			_DebugOption = debugOption;
			_DisableConsoleColorsOption = disableConsoleColorsOption;
			_OutPutPathOptionOption = outPutPathOptionOption;
			_UseVarOverLetOption = UseVarOverLetOption;
		}

		protected override CLICSTOJSOptions GetBoundValue(BindingContext bindingContext) =>
			new()
			{
				Debug = bindingContext.ParseResult.GetValueForOption(_DebugOption),
				DisableConsoleColors = bindingContext.ParseResult.GetValueForOption(_DisableConsoleColorsOption),
				OutPutPath = bindingContext.ParseResult.GetValueForOption(_OutPutPathOptionOption),
				UseVarOverLet = bindingContext.ParseResult.GetValueForOption(_UseVarOverLetOption)
			};
	}
}