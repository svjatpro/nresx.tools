using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Version
{
    [TestFixture]
    public class VersionTests : TestBase
    {
        [TestCase( @"version" )]
        [TestCase( @"-v" )]
        public void WriteVersion( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    var version = ResourceManager.GetVersion();
                    args.ConsoleOutput.Should().BeEquivalentTo($"nresx version: {version}" );
                } );
        }
    }
}