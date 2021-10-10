using System.Threading.Tasks;
using nresx.Tools.Extensions;
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

        [TestCase( "qqqqq\nwwwwww\r\neee", ExpectedResult = "qqqqq\r\nwwwwww\r\neee" )]
        [TestCase( "qqqqq\nwwwwww\neee", ExpectedResult = "qqqqq\r\nwwwwww\r\neee" )]
        [TestCase( "qqqqq\r\nwwwwww\neee", ExpectedResult = "qqqqq\r\nwwwwww\r\neee" )]
        [TestCase( "qqqqq\r\nwwwwww\r\neee", ExpectedResult = "qqqqq\r\nwwwwww\r\neee" )]
        public async Task<string> ReplaceNewLine( string source )
        {
            return source.ReplaceNewLine();
        }
    }
}