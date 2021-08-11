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

            Regex csRegex = new( @"""(.*?)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
            
            var dirSkipList = new HashSet<string>( StringComparer.CurrentCultureIgnoreCase )
            {
                ".git", ".vs", "bin", "obj"
            };

            var elements = new List<ResourceElement>();
            ForEachFile( sourceFiles, context =>
            {
                // filter by skip dirs
                var rootDir = Path.GetDirectoryName( new DirectoryInfo( context.SourcePathSpec ).FullName );
                var relPath = Path.GetRelativePath( rootDir, context.FullName );
                if ( relPath.ContainsDir( dirSkipList.ToArray() ) ) return;

                var nameParts = ( Path.GetDirectoryName( relPath ) ?? "" )
                    .Split( Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar )
                    .Where( p => !string.IsNullOrWhiteSpace( p ) )
                    .ToList();
                var fileName = Path.GetFileNameWithoutExtension( relPath );
                if( !string.IsNullOrWhiteSpace( fileName ) )
                    nameParts.Add( fileName );
                var elNamePath = nameParts.Any() ? string.Join( '_', nameParts ) : "";
                var elIndex = 1;
                var prevIndex = 0;
                
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
                            var value = match.Groups[1].Value;
                            if ( string.IsNullOrWhiteSpace( value ) ) continue;
                            string key = null;

                            // try to get property name
                            Regex csPropRegex = new( $@"[$\s;]+([a-zA-Z0-9_]+)\s*=\s*""{value}""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
                            var propMatch = csPropRegex.Match( fileBody, prevIndex, match.Length + match.Index - prevIndex + 1 );
                            if ( propMatch.Success )
                            {
                                key = propMatch.Groups[1].Value;
                            }

                            // get name from value
                            if ( key == null )
                            {
                                key = value.Split( ' ' ).FirstOrDefault();
                            }

                            elements.Add( new ResourceElement
                            {
                                Key = $"{elNamePath}_{key ?? (elIndex++).ToString()}",
                                Value = value
                            } );
                        }
                        break;
                    }
                }
            } );

            foreach ( var el in elements )
            {
                Console.WriteLine( $"\"{el.Value}\" string has been extracted to \"{el.Key}\" resource element" );
            }
        }
    }
}