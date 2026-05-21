using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Verbose
{
    // Covers the --verbose flag (RSX-152). Asserts that diagnostics only appear when
    // -V / --verbose is passed and that they carry the actually useful info: matched
    // files, formats, destination writes.
    [TestFixture]
    public class VerboseTests : TestBase
    {
        [Test]
        public void NoVerboseFlagDoesNotEmitVerboseLines()
        {
            var args = TestHelper.RunCommandLine( "info [TmpFile]" );
            args.ConsoleOutput.Should().NotContain( line => line.StartsWith( "[verbose]" ) );
        }

        [TestCase( "-V" )]
        [TestCase( "--verbose" )]
        public void VerboseFlagEmitsMatchedFileLine( string flag )
        {
            var args = TestHelper.RunCommandLine( $"info [TmpFile.resx] {flag}" );
            args.ConsoleOutput.Should().Contain( line =>
                line.StartsWith( "[verbose] matched:" ) && line.Contains( "format: Resx" ) );
        }

        [Test]
        public void VerboseLogsSearchPatternAndRecursiveFlag()
        {
            var args = TestHelper.RunCommandLine( @"info [Output]\nonexistent*.resx -V" );
            args.ConsoleOutput.Should().Contain( line =>
                line.StartsWith( "[verbose] searching" ) && !line.Contains( "recursive" ) );

            var argsRec = TestHelper.RunCommandLine( @"info [Output]\nonexistent*.resx -r -V" );
            argsRec.ConsoleOutput.Should().Contain( line =>
                line.StartsWith( "[verbose] searching" ) && line.Contains( "(recursive)" ) );
        }

        [Test]
        public void VerboseLogsDestinationWriteForConvert()
        {
            var args = TestHelper.RunCommandLine( "convert [TmpFile.resx] -f yaml -V --dry-run" );
            args.ConsoleOutput.Should().Contain( line =>
                line.StartsWith( "[verbose] writing:" ) && line.Contains( "format: Yaml" ) );
        }
    }
}
