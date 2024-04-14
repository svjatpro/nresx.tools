using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
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
            
            args.ConsoleOutput[0].Should().Be( $"EmptyValue: {res.Elements[1].Key};" );
        }

        [TestCase( @"validate [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidateEmptyKeyElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[1].Key, "" );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ConsoleOutput[0].Should().Be( $"EmptyKey: (value: {res.Elements[1].Value});" );
        }

        [TestCase( @"validate [TmpFile.json]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidateDuplicatedElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[2].Key, res.Elements[1].Key );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );
            args.ConsoleOutput[0].Should().Be( $"Duplicate: {res.Elements[1].Key};" );
        }

        [TestCase( @"validate [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidatePossibleDuplicatedElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[2].Key, $"{res.Elements[1].Key}.Text" );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ConsoleOutput[0].Should().Be( $"PossibleDuplicate: {res.Elements[1].Key}.Text;" );
        }
    }
}