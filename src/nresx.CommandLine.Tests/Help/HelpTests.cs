using System.Collections.Generic;
using CommandLine;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Help;

[TestFixture]
public class HelpTests : TestBase
{
    [TestCase( @"" )]
    [TestCase( @"help" )]
    [TestCase( @"-h" )]
    //public void WriteGeneralHelp( string commandLine )
    //{
    //    commandLine
    //        .ValidateRun( _ => { } )
    //        .ValidateStdout( args =>
    //        {
    //            args.ConsoleOutput.Should().BeEquivalentTo($"nresx version: {ResourceManager.GetVersion()}" );
    //        } );
    //}

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

                args.ConsoleOutput.Should().BeEquivalentTo(result);
            } );
    }
}