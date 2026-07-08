using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.ExitCodes
{
    // Verifies the documented exit-code table in src/nresx.CommandLine/README.md (RSX-151).
    // Keep the cases here in sync with that table.
    [TestFixture]
    public class ExitCodeTests : TestBase
    {
        [Test]
        public void SuccessfulRunReturnsZero()
        {
            var args = TestHelper.RunCommandLine( "info [TmpFile]" );
            args.ExitCode.Should().Be( ExitSuccess );
        }

        [Test]
        public void InputFileNotFoundReturnsExitNotFound()
        {
            var args = TestHelper.RunCommandLine( "info nonexistent.resx" );
            args.ExitCode.Should().Be( ExitNotFound );
        }

        [Test]
        public void DirectoryNotFoundReturnsExitNotFound()
        {
            var args = TestHelper.RunCommandLine( @"info [UniqueKey]\foo.resx" );
            args.ExitCode.Should().Be( ExitNotFound );
        }

        [Test]
        public void UnknownFormatReturnsExitFormatError()
        {
            var path = GetTestPath( TestData.WrongFormatResourceFile );
            var args = TestHelper.RunCommandLine( $"info {path}" );
            args.ExitCode.Should().Be( ExitFormatError );
        }

        [Test]
        public void ElementNotFoundReturnsExitNotFound()
        {
            var args = TestHelper.RunCommandLine( "rename [TmpFile] -k [UniqueKey] -n [UniqueKey]" );
            args.ExitCode.Should().Be( ExitNotFound );
        }

        [Test]
        public void DestinationConflictReturnsExitDestinationConflict()
        {
            // copy without --new-file when destination doesn't exist
            var args = TestHelper.RunCommandLine( @"copy [TmpFile] [Output]\[UniqueKey].resx" );
            args.ExitCode.Should().Be( ExitDestinationConflict );
        }

        [Test]
        public void MissingRequiredOptionReturnsExitUsageError()
        {
            // rename without --key argument
            var args = TestHelper.RunCommandLine( "rename [TmpFile] -n newkey" );
            args.ExitCode.Should().Be( ExitUsageError );
        }
    }
}
