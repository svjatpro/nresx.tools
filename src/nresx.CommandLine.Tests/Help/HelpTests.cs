using System.Collections.Generic;
using CommandLine;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Help;

[TestFixture]
public class HelpTests : TestBase
{
    private const string DetailedHelpFooter =
        "Use 'nresx help <command>' or 'nresx <command> --help' for detailed help.";

    [TestCase( @"" )]
    [TestCase( @"help" )]
    [TestCase( @"-h" )]
    [TestCase( @"help -a" )]
    public void WriteCommandListTests( string commandLine )
    {
        commandLine
            .ValidateRun( _ => { } )
            .ValidateStdout( args =>
            {
                var result = new List<string> { "Main Commands:" };
                var context = new CommandLineContext();
                foreach (var t in context.CommandTypes)
                {
                    var verb = t.GetCustomAttributes(typeof(VerbAttribute), false);
                    if (verb.Length <= 0) continue;

                    var v = (VerbAttribute)verb[0];
                    result.Add( $"  {v.Name,-12} {v.HelpText}" );
                }
                result.Add( "" );
                result.Add( DetailedHelpFooter );

                args.ConsoleOutput.Should().BeEquivalentTo(result);
            } );
    }

    [Test]
    public void HelpCommandShowsFooter()
    {
        @"help"
            .ValidateRun( args =>
            {
                args.ExitCode.Should().Be( ExitSuccess );
                args.ConsoleOutput.Should().Contain( DetailedHelpFooter );
            } );
    }

    [Test]
    public void HelpForCommandShowsOptionsAndExamples()
    {
        @"help convert"
            .ValidateRun( args =>
            {
                args.ExitCode.Should().Be( ExitSuccess );
                args.ConsoleOutput.Should().Contain( "convert - Convert to another format" );
                args.ConsoleOutput.Should().Contain( "Options:" );
                args.ConsoleOutput.Should().Contain( line => line.Contains( "--source" ) );
                args.ConsoleOutput.Should().Contain( "Examples:" );
                args.ConsoleOutput.Should().Contain( line => line.Contains( "nresx convert" ) );
            } );
    }

    [Test]
    public void HelpForCommandMatchesCommandFlag()
    {
        List<string> viaFlag = null;
        @"convert --help".ValidateRun( args => viaFlag = args.ConsoleOutput );
        @"help convert".ValidateRun( args => args.ConsoleOutput.Should().Equal( viaFlag ) );
    }

    [Test]
    public void HelpForUnknownCommandFailsAndListsCommands()
    {
        @"help nosuch"
            .ValidateRun( args =>
            {
                args.ExitCode.Should().Be( ExitUsageError );
                args.ConsoleOutput.Should().Contain( string.Format( UnknownCommandErrorMessage, "nosuch" ) );
                args.ConsoleOutput.Should().Contain( "Main Commands:" );
                args.ConsoleOutput.Should().Contain( DetailedHelpFooter );
            } );
    }
}
