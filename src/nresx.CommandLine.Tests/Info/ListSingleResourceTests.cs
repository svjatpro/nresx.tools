using System;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.List
{
    [TestFixture]
    public class ListSingleResourceTests : TestBase
    {
        [TestCase( @"list [SourceFile]", "\\k: \\v" )]
        [TestCase( @"list -s [SourceFile]", "\\k: \\v" )]
        [TestCase( @"list --source [SourceFile]", "\\k: \\v" )]
        [TestCase( @"list [SourceFile] -t ""prefix \k: \v postfix""", "prefix \\k: \\v postfix" )]
        [TestCase( @"list [SourceFile] -t ""prefix \k: '\v'; (\c)""", "prefix \\k: '\\v'; (\\c)" )]
        public void GetSingleFileInfo( string commandLine, string format )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    // element values may be multi-line; the product normalizes embedded newlines
                    // to Environment.NewLine, so join with it rather than a hardcoded \r\n.
                    var output = string.Join( Environment.NewLine, args.ConsoleOutput );
                    var elements = string.Join( Environment.NewLine, GetExampleResourceFile()
                        .Elements.Select( el => format
                            .Replace( "\\k", el.Key )
                            .Replace( "\\v", el.Value )
                            .Replace( "\\c", el.Comment ) ) );

                    output.Should().Be( elements );
                } );
        }
    }
}