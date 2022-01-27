using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Tools;

namespace nresx.Core.Tests
{
    public class TestBase
    {
        protected const string FilesNotFoundErrorMessage = "fatal: path mask '{0}' did not match any files";    
        protected const string FileLoadErrorMessage = "fatal: invalid file: '{0}' can't load resource file";
        protected const string DirectoryNotFoundErrorMessage = "fatal: Invalid path: '{0}': no such file or directory";
        protected const string FormatUndefinedErrorMessage = "fatal: resource format is not defined";
        protected const string FileAlreadyExistErrorMessage = "fatal: file '{0}' already exist";

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

        [Obsolete( "use TestHelper.Run() instead" )]
        protected CommandLineParameters Run( string cmdLine, CommandLineParameters parameters = null )
        {
            return TestHelper.RunCommandLine( cmdLine, parameters );
        }

        protected void ValidateElements( ResourceFile resource, ResourceFormatType sourceType = ResourceFormatType.NA )
        {
            var emptyKeyTypes = new[] {ResourceFormatType.PlainText};
            var emptyComTypes = new[] {ResourceFormatType.PlainText, ResourceFormatType.Yaml, ResourceFormatType.Yml};

            var elements = resource.Elements.ToList();
            var validateKey = !emptyKeyTypes.Contains( resource.FileFormat );
            var validateComment = 
                //!( elements.All( el => string.IsNullOrWhiteSpace( el.Comment ) ) && ( emptyComTypes.Contains( resource.FileFormat ) || emptyComTypes.Contains( sourceType ) ) );
                !elements.All( el => string.IsNullOrWhiteSpace( el.Comment ) ) || ( !emptyComTypes.Contains( resource.FileFormat ) && !emptyComTypes.Contains( sourceType ) );

            var actual = elements
                .Select( e => ( 
                    key: validateKey ? (e.Key ?? string.Empty) : string.Empty, 
                    val: e.Value ?? string.Empty, 
                    comment: e.Comment ?? string.Empty) ); 
            var target = GetExampleResourceFile().Elements
                .Select( e => ( 
                    key: validateKey ? (e.Key ?? string.Empty) : string.Empty, 
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

        [Obsolete( "use TestHelper.CopyTemporaryFile() instead" )]
        protected string CopyTemporaryFile(
            string sourcePath = null, 
            string destPath = null,
            string destDir = null,
            ResourceFormatType copyType = ResourceFormatType.Resx )
        {
            return TestHelper.CopyTemporaryFile( sourcePath, destPath, destDir, copyType );
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
                    var localeDir = !string.IsNullOrWhiteSpace( dir ) ? $"{dir}\\{locales[i]}" : $"{locales[i]}";
                    filePath = GetOutputPath( $"{localeDir}\\{fileKey}_{locales[i]}.resx" );
                }
                else
                {
                    filePath =
                        !string.IsNullOrWhiteSpace( dir ) ? 
                        GetOutputPath( $"{dir}\\{fileKey}_{locales[i]}.resx" ) :
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

                var filePath = GetOutputPath( $"{dirKey}\\{fileKey}_{TestData.UniqueKey()}.resx" );
                TestHelper.CopyTemporaryFile( destPath: filePath );

                result.Add( filePath );
            }

            return result;
        }
    }
}