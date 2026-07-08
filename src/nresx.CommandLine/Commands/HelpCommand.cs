using System;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;

namespace nresx.CommandLine.Commands;

[Verb("help", HelpText = "Show help for a command")]
public class HelpCommand : BaseCommand
{
    [Option('a', "all", HelpText = "List all commands", Required = false)]
    public bool ListAll { get; set; }

    protected override bool IsDryRunAllowed => false;

    protected override void ExecuteCommand()
    {
        // `nresx help <command>` - detailed help for a single command (same output as
        // `nresx <command> --help`). Extra positional args beyond the first are ignored.
        var target = Args?.FirstOrDefault();
        if ( !string.IsNullOrWhiteSpace( target ) )
        {
            var cmdType = Context.CommandTypes.FirstOrDefault( t =>
                string.Equals(
                    ( t.GetCustomAttributes( typeof(VerbAttribute), false ).FirstOrDefault() as VerbAttribute )?.Name,
                    target, StringComparison.CurrentCultureIgnoreCase ) );
            if ( cmdType != null )
            {
                HelpRenderer.Render( cmdType );
                return;
            }

            WriteError( ExitUsageError, UnknownCommandErrorMessage, target );
            WriteCommandList();
            return;
        }

        WriteCommandList();
    }

    private void WriteCommandList()
    {
        Console.WriteLine( "Main Commands:" );
        foreach ( var t in Context.CommandTypes )
        {
            var verb = t.GetCustomAttributes( typeof(VerbAttribute), false );
            if ( verb.Length > 0 )
            {
                var v = (VerbAttribute)verb[0];
                Console.WriteLine( $"  {v.Name,-12} {v.HelpText}" );
            }
        }

        Console.WriteLine();
        Console.WriteLine( "Use 'nresx help <command>' or 'nresx <command> --help' for detailed help." );
    }
}
