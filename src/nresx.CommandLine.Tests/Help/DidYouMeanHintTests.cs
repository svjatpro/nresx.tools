using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Help
{
    // The "-r" hint (RSX-233, issue #3): when a wildcard pathspec matches nothing and
    // -r was not given, suggest recursion on stderr next to the not-found error.
    // Exit code stays 3. All assertions use Contain - stderr and stdout interleave.
    [TestFixture]
    public class DidYouMeanHintTests : TestBase
    {
        [TestCase( @"info [Output]/[UniqueKey]*.resx" )]
        [TestCase( @"validate [Output]/[UniqueKey]*.resx" )]
        [TestCase( @"add [Output]/[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey]" )]
        public void HintShownForWildcardWithoutRecursive( string commandLine )
        {
            // .test_output always has subdirectories by the time tests run, but create one
            // explicitly so the test does not depend on suite-wide side effects
            Directory.CreateDirectory( Path.Combine( TestData.OutputFolder, TestData.UniqueKey() ) );

            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().Contain( line => line.StartsWith( "fatal: path mask" ) );
            args.ConsoleOutput.Should().Contain( RecursiveHintMessage );
        }

        [TestCase( @"info [Output]/[UniqueKey]*.resx -r" )]
        [TestCase( @"info [Output]/[UniqueKey]*.resx --recursive" )]
        public void HintNotShownWhenRecursiveAlreadyGiven( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().Contain( line => line.StartsWith( "fatal: path mask" ) );
            args.ConsoleOutput.Should().NotContain( RecursiveHintMessage );
        }

        [TestCase( @"info [Output]/[NewDir]/[UniqueKey]*.resx" )]
        public void HintNotShownWhenNoSubdirectories( string commandLine )
        {
            // [NewDir] creates a fresh empty directory - no subdirectories to search
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().Contain( line => line.StartsWith( "fatal: path mask" ) );
            args.ConsoleOutput.Should().NotContain( RecursiveHintMessage );
        }

        [TestCase( @"info [Output]/[UniqueKey].resx" )]
        public void HintNotShownForExactFileName( string commandLine )
        {
            // -r does not search subdirectories for an exact file name, so the hint
            // would be misleading here
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().Contain( line => line.StartsWith( "fatal: path mask" ) );
            args.ConsoleOutput.Should().NotContain( RecursiveHintMessage );
        }

        [TestCase( @"info [UniqueKey]/[UniqueKey]*.resx" )]
        public void HintNotShownForMissingDirectory( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().Contain( line => line.StartsWith( "fatal: path" ) );
            args.ConsoleOutput.Should().NotContain( RecursiveHintMessage );
        }
    }
}
