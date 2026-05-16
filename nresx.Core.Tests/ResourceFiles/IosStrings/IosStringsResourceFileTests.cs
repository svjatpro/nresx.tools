using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.IosStrings
{
    [TestFixture]
    public class IosStringsResourceFileTests : TestBase
    {
        [Test]
        public void LoadIosStrings_HasExpectedElements()
        {
            var res = new ResourceFile( GetTestPath( "Resources.strings" ) );

            res.FileFormat.Should().Be( ResourceFormatType.IosStrings );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [Test]
        public void Load_BlockCommentBeforeEntry_AttachesAsElementComment()
        {
            var text = "/* hello comment */\n\"hello\" = \"Hi\";\n\"goodbye\" = \"Bye\";";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.IosStrings );

            res.Elements["hello"]!.Comment.Should().Be( "hello comment" );
            res.Elements["goodbye"]!.Comment.Should().BeEmpty();
        }

        [Test]
        public void Load_LineCommentBeforeEntry_AttachesAsElementComment()
        {
            var text = "// a quick note\n\"key\" = \"value\";";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.IosStrings );

            res.Elements["key"]!.Comment.Should().Be( "a quick note" );
        }

        [Test]
        public void Load_EscapeSequences_AreUnescaped()
        {
            var text = "\"quote\" = \"He said \\\"hi\\\"\";\n\"tab\" = \"a\\tb\";\n\"backslash\" = \"a\\\\b\";";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.IosStrings );

            res.Elements["quote"]!.Value.Should().Be( "He said \"hi\"" );
            res.Elements["tab"]!.Value.Should().Be( "a\tb" );
            res.Elements["backslash"]!.Value.Should().Be( @"a\b" );
        }

        [Test]
        public void Save_EscapesSpecialChars()
        {
            var res = new ResourceFile( ResourceFormatType.IosStrings );
            res.Elements.Add( "quote", "He said \"hi\"" );
            res.Elements.Add( "backslash", @"a\b" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.IosStrings );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( "\"quote\" = \"He said \\\"hi\\\"\";" );
            text.Should().Contain( "\"backslash\" = \"a\\\\b\";" );
        }

        [Test]
        public void RoundTrip_KeysAndValuesAndCommentsMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.strings" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.IosStrings );
            source.Save( midPath, ResourceFormatType.IosStrings );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }
    }
}
