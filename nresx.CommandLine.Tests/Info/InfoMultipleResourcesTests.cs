using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Info
{
    [TestFixture]
    public class InfoMultipleResourcesTests : TestBase
    {
        private void ValidateOutputInfo(
            List<string> consoleOutput, 
            int startIndex, 
            string fileName, 
            string path, 
            int elementsCount, 
            ResourceFormatType formatType = ResourceFormatType.Resx )
        {
            consoleOutput[startIndex + 0].Should().Be( $"Resource file name: \"{fileName}\", (\"{path})\"" );
            consoleOutput[startIndex + 1].Should().Be( $"resource format type: {formatType}" );
            consoleOutput[startIndex + 2].Should().Be( $"text elements: {elementsCount}" );
        }

        [TestCase( @"[TmpFile] [TmpFile] [TmpFile]" )]
        [TestCase( @"info [TmpFile] [TmpFile] [TmpFile]" )]
        [TestCase( @"info -s [TmpFile.yaml] [TmpFile.resx]" )]
        [TestCase( @"info --source [TmpFile] [TmpFile]" )]
        public void GetListOfFilesInfo( string commandLine )
        {
            var args = Run( commandLine );

            for ( var i = 0; i < args.TemporaryFiles.Count; i++ )
            {
                var res = new ResourceFile( args.TemporaryFiles[i] );

                ValidateOutputInfo( args.ConsoleOutput, i * 4, res.Name, res.AbsolutePath, res.Elements.Count(), res.ResourceFormat );
            }
        }

        [TestCase( @"[Output]\\[UniqueKey]*.resx" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx" )]
        public void GetFilesInfoByMask( string commandLine )
        {
            var elCount = GetExampleResourceFile().Elements.Count();
            var fileKey = UniqueKey();
            var dirKey = UniqueKey();
            new DirectoryInfo( Path.Combine( TestData.OutputFolder, dirKey ) ).Create();
            var filePath1 = GetOutputPath( $"{fileKey}_11.resx" );
            var filePath2 = GetOutputPath( $"{fileKey}_2.resx" );
            var filePath3 = GetOutputPath( $"{dirKey}\\{fileKey}_33.resx" );

            CopyTemporaryFile( destPath: filePath1 );
            CopyTemporaryFile( destPath: filePath2 );
            CopyTemporaryFile( destPath: filePath3 );

            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { fileKey }} );

            args.ConsoleOutput.Count.Should().Be( 8 );

            ValidateOutputInfo( args.ConsoleOutput, 0, Path.GetFileName( filePath1 ), Path.GetFullPath( filePath1 ), 3 );
            ValidateOutputInfo( args.ConsoleOutput, 4, Path.GetFileName( filePath2 ), Path.GetFullPath( filePath2 ), 3 );
        }

        [TestCase( @"[Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info -s [Output]\\[UniqueKey]*.resx --recursive" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx -r" )]
        [TestCase( @"info --source [Output]\\[UniqueKey]*.resx --recursive" )]
        public void GetFilesInfoByMaskRecursive( string commandLine )
        {
            var elCount = GetExampleResourceFile().Elements.Count();
            var fileKey = UniqueKey();
            var dirKey = UniqueKey();
            new DirectoryInfo( Path.Combine( TestData.OutputFolder, dirKey ) ).Create();
            var filePath1 = GetOutputPath( $"{fileKey}_11.resx" );
            var filePath2 = GetOutputPath( $"{fileKey}_2.resx" );
            var filePath3 = GetOutputPath( $"{dirKey}\\{fileKey}_33.resx" );

            CopyTemporaryFile( destPath: filePath1 );
            CopyTemporaryFile( destPath: filePath2 );
            CopyTemporaryFile( destPath: filePath3 );

            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey } } );

            args.ConsoleOutput.Count.Should().Be( 12 );

            ValidateOutputInfo( args.ConsoleOutput, 0, Path.GetFileName( filePath1 ), Path.GetFullPath( filePath1 ), 3 );
            ValidateOutputInfo( args.ConsoleOutput, 4, Path.GetFileName( filePath2 ), Path.GetFullPath( filePath2 ), 3 );
            ValidateOutputInfo( args.ConsoleOutput, 8, Path.GetFileName( filePath3 ), Path.GetFullPath( filePath3 ), 3 );
        }
    }
}