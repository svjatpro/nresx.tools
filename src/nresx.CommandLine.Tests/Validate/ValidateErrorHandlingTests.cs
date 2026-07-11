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
