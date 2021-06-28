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

        protected string GetTestPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
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

        protected void ValidateElements( ResourceFile resource )
        {
            var elements = resource.Elements.ToList();

            elements
                .Select( e => (key: e.Key, val: e.Value) )
                .Should().BeEquivalentTo(
                    (key: "Entry1.Text", val: "Value1"),
                    (key: "Entry2", val: "Value2"),
                    (key: "Entry3", val: "Value3\r\nmultiline") );
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