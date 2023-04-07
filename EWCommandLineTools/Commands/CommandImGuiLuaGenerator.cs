using System.Runtime.InteropServices.ComTypes;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Pixpil.ImGuiLuaBindingsGenerator;


namespace EWCommandLineTools.Commands;

[Command( name: "imgui", Description = "Calculates the logarithm of a value." )]
public class CommandImGuiLuaGenerator : ICommand {

	// Order: 0
	[CommandParameter( 0, Description = "Value whose logarithm is to be found." )]
	public string ImGuiPath { get; init; }

	// Name: --base
	// Short name: -b
	[CommandParameter( 1, Description = "Logarithm base." )]
	public string OutputPath { get; init; }
	
	[CommandOption( "enum", 'e' )]
	public bool PrintEnum { get; init; }

	public ValueTask ExecuteAsync( IConsole console ) {

		if ( OperatingSystem.IsMacOS() ) {
			Environment.SetEnvironmentVariable( 
				"DYLD_PRINT_LIBRARIES",
				"/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/lib"
			);
		}
		
		Console.WriteLine( $"start parsing: {ImGuiPath}" );
		string parsed = Generator.Parse( ImGuiPath );
		File.WriteAllText( OutputPath, parsed );
		
		return default;
	}
}