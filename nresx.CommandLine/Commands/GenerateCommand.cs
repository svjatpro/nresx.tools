using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "generate", HelpText = "Generate resource file(s) from source code" )]
    public class GenerateCommand : BaseCommand
    {
        protected override bool IsRecursiveAllowed => true;

        #region Private methods

        private bool ValidateFile( string path )
        {
            //

            return true;
        }

        private bool ValidateLine( string line )
        {
            Regex asmRegex = new( @"\[assembly:", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
            Regex commentRegex = new( @"^\s*///", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
            Regex throwRegex = new( @"\s+throw new ", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );

            // [assembly: AssemblyTitle("appUwp")]

            if ( asmRegex.IsMatch( line ) ||
                 commentRegex.IsMatch( line ) ||
                 throwRegex.IsMatch( line ) )
            {
                return false;
            }

            return true;
        }

        #endregion

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

            var elementsMap = new Dictionary<string, List<ResourceElement>>();
            ForEachFile( sourceFiles, context =>
            {
                if ( !ValidateFile( context.PathSpec ) )
                    return;

                var elements = new List<ResourceElement>();
                elementsMap.Add( context.FullName, elements );

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
                        //var fileBody = reader.ReadToEnd();
                        while ( !reader.EndOfStream )
                        {
                            var line = reader.ReadLine();
                            if ( !ValidateLine( line ) )
                                continue;
                            var matches = csRegex.Matches( line );
                            foreach ( Match match in matches )
                            {
                                var value = match.Groups[1].Value;
                                if ( string.IsNullOrWhiteSpace( value ) ) continue;
                                string key = null;

                                // try to get property name
                                Regex csPropRegex = new( $@"[$\s;]+([a-zA-Z0-9_]+)\s*=\s*""{value}""",
                                    RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline |
                                    RegexOptions.CultureInvariant );
                                var propMatch = csPropRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
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
                                    Key = $"{elNamePath}_{key ?? ( elIndex++ ).ToString()}",
                                    Value = value
                                } );
                            }
                        }
                        break;
                    }
                }
            } );

            foreach ( var sourceFile in elementsMap.Keys )
            {
                foreach ( var el in elementsMap[sourceFile] )
                {
                    Console.WriteLine( $"\"{sourceFile.GetShortPath()}\": \"{el.Value}\" string has been extracted to \"{el.Key}\" resource element" );
                }
            }
        }
    }
}