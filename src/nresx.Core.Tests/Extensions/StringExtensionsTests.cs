using System;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Core.Extensions;
using NUnit.Framework;

namespace nresx.Core.Tests.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase( "qqqqq\nwwwwww\r\neee", ExpectedResult = new[]{ "qqqqq", "wwwwww", "eee" } )]
        [TestCase( "qqqqq\nwwwwww\r\ne", ExpectedResult = new[]{ "qqqqq", "wwwwww", "e" } )]
        [TestCase( "qqqqq\nwwwwww\r\n", ExpectedResult = new[]{ "qqqqq", "wwwwww", "" } )]
        [TestCase( "qqqqq\n\r\neee", ExpectedResult = new[] { "qqqqq", "", "eee" } )]
        [TestCase( "\nwwwwww\r\neee", ExpectedResult = new[] { "", "wwwwww", "eee" } )]
        public async Task<string[]> SplitLines( string source )
        {
            return source.SplitLines();
        }

        // ReplaceNewLine normalizes line endings to Environment.NewLine, so build the
        // expected value from it rather than a hardcoded \r\n literal (cross-platform).
        [TestCase( "qqqqq\nwwwwww\r\neee" )]
        [TestCase( "qqqqq\nwwwwww\neee" )]
        [TestCase( "qqqqq\r\nwwwwww\neee" )]
        [TestCase( "qqqqq\r\nwwwwww\r\neee" )]
        public void ReplaceNewLine( string source )
        {
            var expected = $"qqqqq{Environment.NewLine}wwwwww{Environment.NewLine}eee";
            source.ReplaceNewLine().Should().Be( expected );
        }
    }
}