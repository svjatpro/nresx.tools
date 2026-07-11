using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace nresx.CommandLine.Commands.Base
{
    // Renders detailed per-command help: the verb summary, its options, and the
    // Examples block (from BaseCommand.HelpExamples). Shared by both spellings of
    // detailed help - `nresx <command> --help` (routed in Program.cs) and
    // `nresx help <command>` (HelpCommand) - so the two stay identical by construction.
    //
    // Options are read by reflecting the [Option] attributes up the type hierarchy
    // rather than via HelpText.AutoBuild: CommandLineParser treats a [Verb]-decorated
    // type as a verb collection and would render the verb list, not the command's own
    // options.
    internal static class HelpRenderer
    {
        public static void Render( Type commandType )
        {
            var verb = commandType.GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute;
            var verbName = verb?.Name ?? commandType.Name.ToLowerInvariant();

            Console.WriteLine(
                string.IsNullOrWhiteSpace( verb?.HelpText ) ? verbName : $"{verbName} - {verb.HelpText}" );

            // command-specific options first, then the inherited common options
            var options = new List<(string flags, string help)>();
            var seen = new HashSet<string>( StringComparer.Ordinal );
            for ( var t = commandType; t != null && t != typeof(object); t = t.BaseType )
            {
                foreach ( var prop in t.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) )
                {
                    if ( prop.GetCustomAttribute( typeof(OptionAttribute) ) is not OptionAttribute opt || opt.Hidden )
                        continue;

                    // a derived class may shadow an inherited option to re-describe it
                    // (e.g. validate's -f/--format, RSX-232); derived-first walk order
                    // means the most-derived declaration wins
                    if ( !seen.Add( $"{opt.ShortName}|{opt.LongName}" ) )
                        continue;

                    var hasShort = !string.IsNullOrEmpty( opt.ShortName );
                    var hasLong = !string.IsNullOrEmpty( opt.LongName );
                    var flags =
                        hasShort && hasLong ? $"-{opt.ShortName}, --{opt.LongName}" :
                        hasLong ? $"--{opt.LongName}" :
                        hasShort ? $"-{opt.ShortName}" :
                        string.Empty;
                    options.Add( (flags, opt.HelpText) );
                }
            }

            if ( options.Any() )
            {
                Console.WriteLine();
                Console.WriteLine( "Options:" );
                var width = options.Max( o => o.flags.Length );
                foreach ( var (flags, help) in options )
                    Console.WriteLine( $"  {flags.PadRight( width )}  {help}".TrimEnd() );
            }

            var examples =
                ( Activator.CreateInstance( commandType ) as BaseCommand )?.GetHelpExamples().ToList()
                ?? new List<string>();
            if ( examples.Any() )
            {
                Console.WriteLine();
                Console.WriteLine( "Examples:" );
                for ( var i = 0; i < examples.Count; i++ )
                {
                    if ( i > 0 ) Console.WriteLine();
                    foreach ( var line in examples[i].Split( '\n' ) )
                        Console.WriteLine( $"  {line.TrimEnd( '\r' )}" );
                }
            }
        }
    }
}
