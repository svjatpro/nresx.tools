using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core;

namespace nresx.Core.Tests
{
    public class TestBase
    {
        // Mirror of the templates in nresx.CommandLine BaseCommand.cs - update both together when wording changes (RSX-150).
        protected const string FilesNotFoundErrorMessage =
            "fatal: path mask '{0}' did not match any files. Check the path is correct, or use -r to search subdirectories.";
        protected const string FileLoadErrorMessage =
            "fatal: failed to load resource file '{0}'. The file may be corrupt or in an unrecognized format - pass -f <format> to override format detection.";
        protected const string DirectoryNotFoundErrorMessage =
            "fatal: path '{0}' does not exist. Check the path is correct.";
        protected const string FormatUndefinedErrorMessage =
            "fatal: resource format could not be determined. Pass -f <format> with one of: resx, resw, po, json, yaml, xliff, xml, strings, properties, arb, csv, tsv, xlsx, ini, txt.";
        protected const string FileAlreadyExistErrorMessage =
            "fatal: destination file '{0}' already exists. Move or rename the existing file before running this command.";
        protected const string UnknownCommandErrorMessage =
            "Unknown command: '{0}'";
        protected const string UnknownOutputFormatErrorMessage =
            "Unknown output format: '{0}'. Supported: text, json";

        // Mirror of the exit-code constants in nresx.CommandLine BaseCommand.cs (RSX-151).
        protected const int ExitSuccess = 0;
        protected const int ExitFailure = 1;
        protected const int ExitUsageError = 2;
        protected const int ExitNotFound = 3;
        protected const int ExitFormatError = 4;
        protected const int ExitDestinationConflict = 5;

        protected readonly string ElementsSeparateLine = new( '-', 30 );
        
        public static void CleanOutputDir()
        {
            void RemoveFiles( DirectoryInfo dir, bool removeDir )
            {
                foreach ( var fileInfo in dir.GetFiles() )
                    fileInfo.Delete();
                foreach ( var subDir in dir.GetDirectories() )
                    RemoveFiles( subDir, true );
                
                if( removeDir )
                    dir.Delete();
            }

            var root = new DirectoryInfo( TestData.OutputFolder );
            if ( !root.Exists )
            {
                root.Create();
            }
            else
            {
                RemoveFiles( root, false );
            }
        }

        protected string UniqueKey( int length = 8 )
        {
            return TestData.UniqueKey();
        }

        protected string GetTestPath( string fileName, ResourceFormatType type = ResourceFormatType.NA )
        {
            return TestHelper.GetTestPath( fileName, type );
        }

        protected string GetOutputPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
        {
            return TestHelper.GetOutputPath( fileName, type );
        }

        protected void ValidateElements( ResourceFile resource, ResourceFile example = null )
        {
            var validateKey = resource.ElementHasKey;
            var validateComment = resource.ElementHasComment;

            var elements = resource.Elements.ToList();
            var actual = elements
                .Select( e => ( 
                    key: validateKey ? (e.Key ?? string.Empty) : e.Value, 
                    val: e.Value ?? string.Empty,
                    comment: validateComment ? ( e.Comment ?? string.Empty ) : string.Empty) );
            var target = ( example ?? GetExampleResourceFile() ).Elements
                .Select( e => ( 
                    key: validateKey ? (e.Key ?? string.Empty) : e.Value, 
                    val: e.Value ?? string.Empty, 
                    comment: validateComment ? (e.Comment ?? string.Empty) : string.Empty) );

            actual.Should().BeEquivalentTo( target );
        }
        
        protected ResourceFile GetExampleResourceFile()
        {
            var example = new ResourceFile( GetTestPath( TestData.ExampleResourceFile ) );
            return example;
        }

        protected void AddExampleElements( ResourceFile res )
        {
            var example = GetExampleResourceFile();
            foreach ( var el in example.Elements )
                res.Elements.Add( el.Key, el.Value, el.Comment );
        }

        protected List<string> PrepareGroupedFiles( string[] locales, out string fileKey, bool dirLocales = false, string dir = null )
        {
            fileKey = TestData.UniqueKey();
            var result = new List<string>();

            var baseDir = TestData.OutputFolder;
            if ( !string.IsNullOrWhiteSpace( dir ) )
            {
                baseDir = Path.Combine( TestData.OutputFolder, dir );
                new DirectoryInfo( baseDir ).Create();
            }

            for ( var i = 0; i < locales.Length; i++ )
            {
                string filePath;
                if ( dirLocales )
                {
                    new DirectoryInfo( Path.Combine( baseDir, locales[i] ) ).Create();
                    var localeDir = !string.IsNullOrWhiteSpace( dir ) ? $"{dir}/{locales[i]}" : $"{locales[i]}";
                    filePath = GetOutputPath( $"{localeDir}/{fileKey}_{locales[i]}.resx" );
                }
                else
                {
                    filePath =
                        !string.IsNullOrWhiteSpace( dir ) ? 
                        GetOutputPath( $"{dir}/{fileKey}_{locales[i]}.resx" ) :
                        GetOutputPath( $"{fileKey}_{locales[i]}.resx" );
                }

                TestHelper.CopyTemporaryFile( destPath: filePath );
                result.Add( filePath );
            }

            return result;
        }

        // todo: add "randomResourceType" option, false by default (resx)
        protected List<string> PrepareTemporaryFiles( int rootFiles, int firstDirFiles, out string fileKey, string dir = null )
        {
            fileKey = TestData.UniqueKey();
            var result = new List<string>();

            for ( var i = 0; i < rootFiles; i++ )
            {
                var filePath = GetOutputPath( $"{fileKey}_{TestData.UniqueKey()}.resx" );
                TestHelper.CopyTemporaryFile( destPath: filePath );

                result.Add( filePath );
            }

            for ( var i = 0; i < firstDirFiles; i++ )
            {
                var dirKey = TestData.UniqueKey();
                new DirectoryInfo( Path.Combine( TestData.OutputFolder, dirKey ) ).Create();

                var filePath = GetOutputPath( $"{dirKey}/{fileKey}_{TestData.UniqueKey()}.resx" );
                TestHelper.CopyTemporaryFile( destPath: filePath );

                result.Add( filePath );
            }

            return result;
        }
    }
}