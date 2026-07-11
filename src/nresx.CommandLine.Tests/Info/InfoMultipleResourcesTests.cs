using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Core;
using nresx.Core.Helpers;
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
            var filePath3 = GetOutputPath( $"{dirKey}/{fileKey}_33.resx" );

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

        // Order-insensitive variant: files matched by a mask are processed in OS directory
        // enumeration order, which is alphabetical on Windows but arbitrary on Linux/macOS
        // (RSX-243) - locate the file's block by its header line instead of by position.
        private void ValidateOutputInfoAnyPosition(
            List<string> consoleOutput,
            string fileName,
            string path,
            ResourceFormatType formatType = ResourceFormatType.Resx )
        {
            var elCount = GetExampleResourceFile().Elements.Count();
            var header = $"Resource file name: \"{fileName}\", (\"{path})\"";
            var index = consoleOutput.IndexOf( header );
            index.Should().BeGreaterThanOrEqualTo( 0, $"output should contain the info block for '{fileName}'" );
            consoleOutput[index + 1].Should().Be( $"resource format type: {formatType}" );
            consoleOutput[index + 2].Should().Be( $"text elements: {elCount}" );
        }

        #endregion

        [TestCase( @"info [TmpFile] [TmpFile] [TmpFile]" )]
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

        [TestCase( @"[Output]/[UniqueKey]*.resx" )]
        [TestCase( @"info -s [Output]/[UniqueKey]*.resx" )]
        [TestCase( @"info --source [Output]/[UniqueKey]*.resx" )]
        public void GetFilesInfoByMask( string commandLine )
        {
            var files = PrepareFiles( out var fileKey );
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey } } )
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Count.Should().Be( 7 );
                    ValidateOutputInfoAnyPosition( args.ConsoleOutput, Path.GetFileName( files[0] ), Path.GetFullPath( files[0] ) );
                    ValidateOutputInfoAnyPosition( args.ConsoleOutput, Path.GetFileName( files[1] ), Path.GetFullPath( files[1] ) );
                } );
        }

        [TestCase( @"info [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]/[UniqueKey]*.resx --recursive" )]
        [TestCase( @"info --source [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info --source [Output]/[UniqueKey]*.resx --recursive" )]
        public void GetFilesInfoByMaskRecursive( string commandLine )
        {
            var files = PrepareFiles( out var fileKey );
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey } } )
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Count.Should().Be( 11 );
                    ValidateOutputInfoAnyPosition( args.ConsoleOutput, Path.GetFileName( files[0] ), Path.GetFullPath( files[0] ) );
                    ValidateOutputInfoAnyPosition( args.ConsoleOutput, Path.GetFileName( files[1] ), Path.GetFullPath( files[1] ) );
                    ValidateOutputInfoAnyPosition( args.ConsoleOutput, Path.GetFileName( files[2] ), Path.GetFullPath( files[2] ) );
                } );
        }

        [TestCase( @"info [UniqueKey]/*.resx" )]
        [TestCase( @"info [UniqueKey]/*.resx -r" )]
        [TestCase( @"info -s [UniqueKey]/*.resx" )]
        [TestCase( @"info -s [UniqueKey]/*.resx -r" )]
        [TestCase( @"info --source [UniqueKey]/*.resx -r" )]
        [TestCase( @"info --source [UniqueKey]/*.resx --recursive" )]
        public void GetFilesInfoForWrongDirectory( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"fatal: path '{args.UniqueKeys[0]}/*.resx' does not exist. Check the path is correct." );
                } );
        }

        [TestCase( @"info nonexistent.resx", false )]
        [TestCase( @"info nonexistent*.resx", true )] // wildcard => the -r hint follows (RSX-233)
        [TestCase( @"info *nonexistent", true )]
        public void GetWrongFileSpec( string commandLine, bool expectHint )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().Contain( $"fatal: path mask '{commandLine[5..]}' did not match any files. Check the path is correct." );
                    if ( expectHint )
                        args.ConsoleOutput.Should().Contain( RecursiveHintMessage );
                    else
                        args.ConsoleOutput.Should().NotContain( RecursiveHintMessage );
                } );
        }

        [TestCase( @"info [TmpFile] [UniqueKey] [TmpFile]" )]
        public void GetWrongFileSpecMultiple( string commandLine )
        {
            // Position-tolerant: stderr (the fatal: line, from RSX-150) and stdout
            // (the info blocks + separators) are captured by separate async event
            // handlers, so their relative order in ConsoleOutput is timing-dependent.
            // Assert that each expected line appears somewhere instead.
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    var tmp1 = Path.GetFullPath( args.TemporaryFiles[0] );
                    var tmp2 = Path.GetFullPath( args.TemporaryFiles[1] );
                    var name1 = Path.GetFileName( args.TemporaryFiles[0] );
                    var name2 = Path.GetFileName( args.TemporaryFiles[1] );

                    args.ConsoleOutput.Should().Contain( $"Resource file name: \"{name1}\", (\"{tmp1})\"" );
                    args.ConsoleOutput.Should().Contain( $"Resource file name: \"{name2}\", (\"{tmp2})\"" );
                    args.ConsoleOutput.Should().ContainSingle( line => line.StartsWith( $"fatal: path mask '{args.UniqueKeys[0]}' did not match any files" ) );
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

        [TestCase( @"info [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]/[UniqueKey]*.resx --recursive" )]
        [TestCase( @"info --source [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info --source [Output]/[UniqueKey]*.resx --recursive" )]
        public void GetWrongFileFormatMultiple( string commandLine )
        {
            var files = PrepareFiles( out var fileKey );
            var wrongFile = GetOutputPath( $"{fileKey}_0" );
            new FileInfo( GetTestPath( TestData.WrongFormatResourceFile ) ).CopyTo( wrongFile );

            // Position-tolerant: see GetWrongFileSpecMultiple - stderr (the fatal: line)
            // interleaves with stdout (info blocks + separators) non-deterministically.
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey } } )
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().ContainSingle( line => line.StartsWith( $"fatal: failed to load resource file '{new FileInfo( wrongFile ).FullName}'" ) );
                    args.ConsoleOutput.Should().Contain( $"Resource file name: \"{Path.GetFileName( files[0] )}\", (\"{Path.GetFullPath( files[0] )})\"" );
                    args.ConsoleOutput.Should().Contain( $"Resource file name: \"{Path.GetFileName( files[1] )}\", (\"{Path.GetFullPath( files[1] )})\"" );
                    args.ConsoleOutput.Should().Contain( $"Resource file name: \"{Path.GetFileName( files[2] )}\", (\"{Path.GetFullPath( files[2] )})\"" );
                } );
        }
    }
}