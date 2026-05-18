using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Core;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Validate
{
    [TestFixture]
    public class ValidateSingleResourceTests : TestBase
    {
        [TestCase( @"validate [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidateEmptyValueElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            res.Elements[1].Value = string.Empty;
            res.Save( file );
            
            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters{ TemporaryFiles = { file } } );

            args.ConsoleOutput[0].Should().Be( $"{new FileInfo( file ).FullName}: warning: EmptyValue: {res.Elements[1].Key}" );
        }

        [TestCase( @"validate [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidateEmptyKeyElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true, RequireMangleable = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[1].Key, "" );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ConsoleOutput[0].Should().Be( $"{new FileInfo( file ).FullName}: error: EmptyKey: (value: {res.Elements[1].Value})" );
        }

        [TestCase( @"validate [TmpFile.json]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidateDuplicatedElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true, RequireMangleable = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[2].Key, res.Elements[1].Key );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );
            args.ConsoleOutput[0].Should().Be( $"{new FileInfo( file ).FullName}: error: Duplicate: {res.Elements[1].Key}" );
        }

        [TestCase( @"validate [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidatePossibleDuplicatedElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true, RequireMangleable = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[2].Key, $"{res.Elements[1].Key}.Text" );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ConsoleOutput[0].Should().Be( $"{new FileInfo( file ).FullName}: warning: PossibleDuplicate: {res.Elements[1].Key}.Text" );
        }

        [TestCase( @"validate [TmpFile.resx]" )]
        [TestCase( @"validate [TmpFile.xlsx]" )]
        public void ExitCode_CleanFile_IsZero( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( 0 );
        }

        [TestCase( @"validate [TmpFile]" )]
        public void ExitCode_DuplicateKey_IsNonZero( string commandLine )
        {
            // Duplicate is classified as Error → non-zero exit
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true, RequireMangleable = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[2].Key, res.Elements[1].Key );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ExitCode.Should().NotBe( 0 );
        }

        [TestCase( @"validate [TmpFile]" )]
        public void ExitCode_OnlyWarnings_IsZeroByDefault( string commandLine )
        {
            // EmptyValue is classified as Warning → exit 0 by default
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            res.Elements[1].Value = string.Empty;
            res.Save( file );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ExitCode.Should().Be( 0 );
        }

        [TestCase( @"validate [TmpFile] --warnings-as-errors" )]
        public void ExitCode_WarningsAsErrors_PromotesWarningsToFailure( string commandLine )
        {
            // EmptyValue is a Warning; with --warnings-as-errors → non-zero exit
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            res.Elements[1].Value = string.Empty;
            res.Save( file );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ExitCode.Should().NotBe( 0 );
        }
    }
}