using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    [Verb( "generate", HelpText = "Generate resource file(s) from source code" )]
    public class GenerateCommand : BaseCommand
    {
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
            if ( !optionsParsed )
                return;

            Regex csRegex = new( @""".*?""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );

            var dirSkipList = new HashSet<string>( StringComparer.CurrentCultureIgnoreCase )
            {
                ".git", ".vs", "bin", "obj"
            };

            ForEachFile( sourceFiles, context =>
            {
                // filter by skip dirs
                var rootDir = Path.GetDirectoryName( new DirectoryInfo( context.SourcePathSpec ).FullName );
                var relPath = Path.GetDirectoryName( Path.GetRelativePath( rootDir, context.FullName ) );
                if ( relPath.ContainsDir( dirSkipList.ToArray() ) ) return;
                
                var ext = Path.GetExtension( context.FullName );
                switch ( ext )
                {
                    case ".cs":
                    {
                        using var reader = new StreamReader( new FileStream( context.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );
                        var fileBody = reader.ReadToEnd();
                        var matches = csRegex.Matches( fileBody );
                        foreach ( Match match in matches )
                        {
                            
                        }
                        break;
                    }
                }
            } );
        }
    }
}