using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Info
{
    [TestFixture]
    public class InfoMultipleResourcesTests : TestBase
    {
        #region Private members

        private List<string> PrepareFiles( out string fileKey )
        {
            fileKey = UniqueKey();
            var dirKey = UniqueKey();
            new DirectoryInfo( Path.Combine( TestData.OutputFolder, dirKey ) ).Create();
            var filePath1 = GetOutputPath( $"{fileKey}_11.resx" );
            var filePath2 = GetOutputPath( $"{fileKey}_2.resx" );
            var filePath3 = GetOutputPath( $"{dirKey}\\{fileKey}_33.resx" );

            TestHelper.CopyTemporaryFile( destPath: filePath1 );
            TestHelper.CopyTemporaryFile( destPath: filePath2 );
            TestHelper.CopyTemporaryFile( destPath: filePath3 );

            return new List<string> {filePath1, filePath2, filePath3};
        }

        private void ValidateOutputInfo(
            List<string> consoleOutput, 
            int startIndex, 
            string fileName, 
            string path, 
            ResourceFormatType formatType = ResourceFormatType.Resx )
        {
            var elCount = GetExampleResourceFile().Elements.Count();
            consoleOutput[startIndex + 0].Should().Be( $"Resource file name: \"{fileName}\", (\"{path})\"" );
            consoleOutput[startIndex + 1].Should().Be( $"resource format type: {formatType}" );
            consoleOutput[startIndex + 2].Should().Be( $"text elements: {elCount}" );
        }

        #endregion

        [TestCase( @"[TmpFile] [TmpFile] [TmpFile]" )]
        [TestCase( @"info [TmpFile] [TmpFile] [TmpFile]" )]
        [TestCase( @"info -s [TmpFile.yaml] [TmpFile.resx]" )]
        [TestCase( @"info --source [TmpFile] [TmpFile]" )]
        public void GetListOfFilesInfo( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    for ( var i = 0; i < args.TemporaryFiles.Count; i++ )
                    {
                        var res = new ResourceFile( args.TemporaryFiles[i] );
                        ValidateOutputInfo( args.ConsoleOutput, i * 4, res.FileName, res.AbsolutePath, res.FileFormat );
                    }
                } );
        }

        [TestCase( @"[Output]\\[UniqueKey]*.resx" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx" )]
        public void GetFilesInfoByMask( string commandLine )
        {
            var files = PrepareFiles( out var fileKey );
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey } } )
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Count.Should().Be( 7 );
                    ValidateOutputInfo( args.ConsoleOutput, 0, Path.GetFileName( files[0] ), Path.GetFullPath( files[0] ) );
                    ValidateOutputInfo( args.ConsoleOutput, 4, Path.GetFileName( files[1] ), Path.GetFullPath( files[1] ) );
                } );
        }

        [TestCase( @"[Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx --recursive" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx --recursive" )]
        public void GetFilesInfoByMaskRecursive( string commandLine )
        {
            var files = PrepareFiles( out var fileKey );
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey } } )
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Count.Should().Be( 11 );
                    ValidateOutputInfo( args.ConsoleOutput, 0, Path.GetFileName( files[0] ), Path.GetFullPath( files[0] ) );
                    ValidateOutputInfo( args.ConsoleOutput, 4, Path.GetFileName( files[1] ), Path.GetFullPath( files[1] ) );
                    ValidateOutputInfo( args.ConsoleOutput, 8, Path.GetFileName( files[2] ), Path.GetFullPath( files[2] ) );
                } );
        }

        [TestCase( @"[UniqueKey]\*.resx" )]
        [TestCase( @"[UniqueKey]\*.resx -r" )]
        [TestCase( @"info -s [UniqueKey]\*.resx" )]
        [TestCase( @"info -s [UniqueKey]\*.resx -r" )]
        [TestCase( @"info --source [UniqueKey]\*.resx -r" )]
        [TestCase( @"info --source [UniqueKey]\*.resx --recursive" )]
        public void GetFilesInfoForWrongDirectory( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"fatal: Invalid path: '{args.UniqueKeys[0]}\\*.resx': no such file or directory" );
                } );
        }

        [TestCase( @"nonexisting.resx" )]
        [TestCase( @"nonexisting*.resx" )]
        [TestCase( @"*nonexisting" )]
        public void GetWrongFileSpec( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"fatal: path mask '{commandLine}' did not match any files" );
                } );
        }

        [TestCase( @"[TmpFile] [UniqueKey] [TmpFile]" )]
        public void GetWrongFileSpecMultiple( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    var tmp1 = Path.GetFullPath( args.TemporaryFiles[0] );
                    var tmp2 = Path.GetFullPath( args.TemporaryFiles[1] );

                    ValidateOutputInfo( args.ConsoleOutput, 0, Path.GetFileName( args.TemporaryFiles[0] ), tmp1, ResourceFormatHelper.GetFormatType( tmp1 ) );
                    args.ConsoleOutput[4].Should().StartWith( $"fatal: path mask '{args.UniqueKeys[0]}' did not match any files" );
                    ValidateOutputInfo( args.ConsoleOutput, 6, Path.GetFileName( args.TemporaryFiles[1] ), tmp2, ResourceFormatHelper.GetFormatType( tmp2 ) );
                } );
        }

        [Test]
        public void GetWrongFileFormat()
        {
            var fileName = $"{GetTestPath( TestData.WrongFormatResourceFile )}";
            $"info {fileName}"
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( FormatUndefinedErrorMessage );
                } );
        }

        [TestCase( @"[Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx --recursive" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx --recursive" )]
        public void GetWrongFileFormatMultiple( string commandLine )
        {
            var files = PrepareFiles( out var fileKey );
            var wrongFile = GetOutputPath( $"{fileKey}_0" );
            new FileInfo( GetTestPath( TestData.WrongFormatResourceFile ) ).CopyTo( wrongFile );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey } } )
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Count.Should().Be( 13 );
                    args.ConsoleOutput[0].Should().StartWith( $"fatal: invalid file: '{new FileInfo( wrongFile ).FullName}' can't load resource file" );
                    ValidateOutputInfo( args.ConsoleOutput, 2, Path.GetFileName( files[0] ), Path.GetFullPath( files[0] ) );
                    ValidateOutputInfo( args.ConsoleOutput, 6, Path.GetFileName( files[1] ), Path.GetFullPath( files[1] ) );
                    ValidateOutputInfo( args.ConsoleOutput, 10, Path.GetFileName( files[2] ), Path.GetFullPath( files[2] ) );
                } );
        }
    }
}