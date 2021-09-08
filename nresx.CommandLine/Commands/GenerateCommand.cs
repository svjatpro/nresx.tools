using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.CodeParsers;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "generate", HelpText = "Generate resource file(s) from source code" )]
    public class GenerateCommand : BaseCommand
    {
        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public IEnumerable<string> DestinationFiles { get; set; }

        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;
        protected override bool IsDryRunAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;

        #region Private methods

        private bool ValidateFile( string path )
        {
            //

            return true;
        }

        #endregion

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Single( DestinationFiles, out var destFile, mandatory: !DryRun )
                .Validate();
            if ( !optionsParsed )
                return;

            // generate destination file
            if ( OptionHelper.DetectResourceFormat( Format, out var format ) )
            {
                destFile = Path.ChangeExtension( destFile, Format.ToExtension() );
            }
            ResourceFile destination = null;
            if ( !DryRun )
            {
                destination = new ResourceFile( destFile );
                if ( destination.IsNewFile && !CreateNewFile )
                {
                    Console.WriteLine( FilesNotFoundErrorMessage, destFile.GetShortPath() );
                    return;
                }
            }

            var parsers = new Dictionary<string, ICodeParser>
            {
                { ".cs", new CsCodeParser() },
                { ".xaml", new XamlCodeParser() }
            };
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
                
                var ext = Path.GetExtension( context.FullName );
                if ( ext == null || !parsers.ContainsKey( ext ) ) return;
                var parser = parsers[ext];

                using var reader = new StreamReader( new FileStream( context.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );
                while ( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();
                    var result = parser.ParseLine( line, elNamePath );
                    if ( result.Any() )
                    {
                        elements.AddRange( result.Select( r => new ResourceElement
                        {
                            Key = r.Key,
                            Value = r.Value
                        } ) );
                    }
                }
            } );
            
            // write result to console
            foreach ( var sourceFile in elementsMap.Keys )
            {
                foreach ( var el in elementsMap[sourceFile] )
                {
                    // write to destination file
                    if( !DryRun )
                        destination.Elements.Add( el.Key, el.Value );

                    // write to output
                    Console.WriteLine( $"\"{sourceFile.GetShortPath()}\": \"{el.Value}\" string has been extracted to \"{el.Key}\" resource element" );
                }
            }

            // save destination resource file
            if ( !DryRun )
            {
                destination.Save( destFile );
            }
        }
    }
}