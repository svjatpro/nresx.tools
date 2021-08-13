using FluentAssertions;
using nresx.CommandLine.Tests.Format;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Generate
{
    [TestFixture]
    public class GenerateResourceTests : FormatBasicTests
    {
        [TestCase( @"generate -s .test_projects\appUwp\* -r" )]
        public void FormatSingleFile( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ConsoleOutput.Should().BeEquivalentTo(
                @"""appUwp\MainViewModel.cs"": ""The long description"" string has been extracted to ""MainViewModel_The"" resource element",
                @"""appUwp\MainViewModel.cs"": ""The Button2"" string has been extracted to ""MainViewModel_The"" resource element" );
        }
    }
}