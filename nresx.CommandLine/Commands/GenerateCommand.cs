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

        [Option( "link", HelpText = "Replace raw text with resource tag in source files", Required = false, Default = false )]
        public bool LinkResources { get; set; }

        [Option( "exclude", HelpText = "Define directories, which will be excluded during", Required = false, Default = null )]
        public string ExcludeDir { get; set; }

        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;
        protected override bool IsDryRunAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;

        #region Private methods

        private bool ValidateFile( string path )
        {
            // check if path is null or empty
            return path != null && File.Exists( path );
        }

        #endregion

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true ) 
                .Single( DestinationFiles, out var destFile, mandatory: !DryRun )
                .Validate();
            if ( !optionsParsed )
                return;

            // generate destination file
            if ( OptionHelper.DetectResourceFormat( Format, out _ ) )
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
            var dirSkipList = 
                string.IsNullOrWhiteSpace( ExcludeDir ) ?
                new [] { ".git", ".vs", "bin", "obj" } :
                ExcludeDir
                    .Split( ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
                    .ToArray();
            
            var elementsMap = new Dictionary<string, List<ResourceElement>>();
            ForEachFile( sourceFiles, context =>
            {
                if ( !ValidateFile( context.PathSpec ) )
                    return;

                var elements = new List<ResourceElement>();
                elementsMap.Add( context.FullName, elements );

                // filter by skip dirs
                var rootDir = Path.GetDirectoryName( new DirectoryInfo( context.SourcePathSpec ).FullName );
                var relPath = rootDir != null ? Path.GetRelativePath( rootDir, context.FullName ) : context.FullName;
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

                var tmpFile = Path.Combine( Path.GetTempPath(), Guid.NewGuid().ToString() );
                var fileChanged = false;
                string AddNewElement( string key1, string value1 )
                {
                    fileChanged = true;
                    elements.Add( new ResourceElement { Key = key1, Value = value1 } );
                    return key1;
                }

                using var reader = new StreamReader( new FileStream( context.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );
                using var writer = 
                    ( DryRun ||  !LinkResources ) ? null : 
                    new StreamWriter( new FileStream( tmpFile, FileMode.CreateNew, FileAccess.Write, FileShare.ReadWrite ) );
                while ( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();
                    parser.ProcessNextLine( line, elNamePath,
                        ( key, value ) =>
                        {
                            var existingElement = destination?.Elements.FirstOrDefault( el => el.Key == key );
                            if ( existingElement != null )
                            {
                                //  1. the key already exists, and value is the same, no need to generate new one
                                if ( existingElement.Value == value )
                                    return key;

                                //  2. the key already exists, but value is different, generate new element
                                return AddNewElement( parser.IncrementKey( key, elements ), value );
                            }

                            // 5. try to localize already localized entry
                            if ( destination?.Elements.Any( el => el.Key == value ) ?? false )
                                return null;

                            // 4. the key doesn't exist, but value is the same, reuse existing element
                            var existingValue = destination?.Elements.FirstOrDefault( el => el.Value == value );
                            if ( existingValue != null )
                                return existingValue.Key;

                            // 3. the key doesn't exist, new one
                            return AddNewElement( key, value );
                        },
                        processedLine => writer?.WriteLine( processedLine ) );
                }

                reader.Close();
                if ( !DryRun && LinkResources && fileChanged )
                {
                    writer?.Close();
                    new FileInfo( context.FullName ).Delete(); // todo: rename until new one moved
                    new FileInfo( tmpFile ).MoveTo( context.FullName );
                }

            } );
            
            // write result to console
            foreach ( var sourceFile in elementsMap.Keys )
            {
                foreach ( var el in elementsMap[sourceFile] )
                {
                    var newElement = !(destination?.Elements.Any( e => e.Key == el.Key ) ?? false);
                    // write to destination file
                    if ( !DryRun && newElement )
                    {
                        destination?.Elements.Add( el.Key, el.Value );
                    }

                    // write to output
                    if ( newElement )
                    {
                        Console.WriteLine( $@"""{sourceFile.GetShortPath()}"": ""{el.Value}"" string has been extracted to ""{el.Key}"" resource element" );
                    }
                }
            }

            // save destination resource file
            if ( !DryRun )
            {
                destination?.Save( destFile );
            }
        }
    }
}