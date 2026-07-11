using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Validate
{
    // Bad-input handling for validate (RSX-248, fixed as part of RSX-232): wrong masks,
    // missing directories/files, and unknown formats must produce the standard stderr
    // message + exit code instead of an unhandled-exception crash.
    [TestFixture]
    public class ValidateErrorHandlingTests : TestBase
    {
        [TestCase( @"validate [Output]/[UniqueKey]*.resx" )]
        [TestCase( @"validate -s [Output]/[UniqueKey]*.resx -r" )]
        public void NonMatchingMaskReportsNotFound( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().ContainSingle( line => line.StartsWith( "fatal: path mask" ) );
        }

        [TestCase( @"validate [UniqueKey]/[UniqueKey].resx" )]
        public void NonExistingDirectoryReportsNotFound( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            var spec = $"{args.UniqueKeys[0]}/{args.UniqueKeys[1]}.resx";
            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( DirectoryNotFoundErrorMessage, spec ) );
        }

        [TestCase( @"validate [Output]/[UniqueKey].resx" )]
        public void NonExistingFileReportsNotFound( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            var spec = $"{TestData.OutputFolder}/{args.UniqueKeys[0]}.resx";
            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( FilesNotFoundErrorMessage, spec ) );
        }

        // Bare invocation used to fail SILENTLY with exit 2 (no message at all) - found
        // dogfooding on a real project (RSX-245): the user's literal first move.
        [TestCase( @"validate" )]
        [TestCase( @"info" )]
        public void MissingSourceReportsUsageError( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitUsageError );
            args.ConsoleOutput.Should().Contain( string.Format( MissingOptionMessage, "source" ) );
        }

        // A pathspec that is an existing directory means "everything in it" (RSX-245).
        [TestCase( @"validate [Output]/[NewDir]" )]
        public void DirectoryPathspecValidatesItsFiles( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs );
            var dir = Path.Combine( TestData.OutputFolder, preArgs.NewDirectories[0] );
            TestHelper.CopyTemporaryFile( destPath: Path.Combine( dir, $"{TestData.UniqueKey()}.resx" ) );

            var args = TestHelper.RunCommandLine( commandLine, preArgs );

            args.ExitCode.Should().Be( ExitSuccess );
            args.ConsoleOutput.Should().Contain( "Found 0 issues (1 file checked)" );
        }

        // A clean run must confirm what was checked - silence used to be indistinguishable
        // from "nothing matched" (RSX-245).
        [TestCase( @"validate [TmpFile.resx]" )]
        public void CleanRunPrintsFilesCheckedSummary( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitSuccess );
            args.ConsoleOutput.Should().Contain( "Found 0 issues (1 file checked)" );
        }

        // RSX-251: an ordinary nested i18next file used to flood false Duplicate errors
        // in validate and crash list with an unhandled ValidationException.
        [Test]
        public void NestedJsonValidatesCleanAndLists()
        {
            var file = GetTestPath( "json/i18next.json" );

            var validated = TestHelper.RunCommandLine( $"validate {file}" );
            validated.ExitCode.Should().Be( ExitSuccess );
            validated.ConsoleOutput.Should().Contain( "Found 0 issues (1 file checked)" );

            var listed = TestHelper.RunCommandLine( $"list {file}" );
            listed.ExitCode.Should().Be( ExitSuccess );
            listed.ConsoleOutput.Should().Contain( line => line.StartsWith( "statusSection.title:" ) );
            listed.ConsoleOutput.Should().Contain( line => line.StartsWith( "comment:" ) );
        }

        [Test]
        public void UnknownFormatFileReportsFormatError()
        {
            var target = GetOutputPath( $"{TestData.UniqueKey()}.unknownext" );
            File.Copy( GetTestPath( TestData.ExampleResourceFile ), target );

            var args = TestHelper.RunCommandLine( $"validate {target}" );

            args.ExitCode.Should().Be( ExitFormatError );
            args.ConsoleOutput.Should().ContainSingle( line => line.StartsWith( "fatal: resource format could not be determined" ) );
        }
    }
}
