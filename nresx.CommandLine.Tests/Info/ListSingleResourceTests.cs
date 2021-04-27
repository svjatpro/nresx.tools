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
        [TestCase( @"list [SourceFile] -t ""prefix \k: \v posfix""", "prefix \\k: \\v posfix" )]
        [TestCase( @"list [SourceFile] -t ""prefix \k: '\v'; (\c)""", "prefix \\k: '\\v'; (\\c)" )]
        public void GetSingleFileInfo( string commandLine, string format )
        {
            var args = Run( commandLine );

            var output = string.Join( "\r\n", args.ConsoleOutput );
            var elements = string.Join( "\r\n", GetExampleResourceFile()
                .Elements.Select( el => format
                    .Replace( "\\k", el.Key )
                    .Replace( "\\v", el.Value )
                    .Replace( "\\c", el.Comment ) ) );

            output.Should().Be( elements );
        }
    }
}