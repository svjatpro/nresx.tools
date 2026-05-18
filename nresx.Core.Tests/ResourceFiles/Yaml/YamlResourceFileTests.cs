using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Yaml
{
    [TestFixture]
    public class YamlResourceFileTests : TestBase
    {
        [Test]
        public void Load_CommentAboveKey_AttachesAsElementComment()
        {
            var yaml = @"# greeting for first-time users
hello: ""Hi""
goodbye: ""Bye""";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( yaml ) );
            var res = new ResourceFile( stream, ResourceFormatType.Yaml );

            res.Elements["hello"]!.Comment.Should().Be( "greeting for first-time users" );
            res.Elements["goodbye"]!.Comment.Should().BeEmpty();
        }

        [Test]
        public void Load_MultiLineCommentBlock_JoinsLines()
        {
            var yaml = @"# first line
# second line
key: value";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( yaml ) );
            var res = new ResourceFile( stream, ResourceFormatType.Yaml );

            res.Elements["key"]!.Comment.Replace( "\r\n", "\n" ).Should().Be( "first line\nsecond line" );
        }

        [Test]
        public void Load_BlankLineBetweenCommentAndKey_DropsTheComment()
        {
            var yaml = @"# stray comment

key: value";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( yaml ) );
            var res = new ResourceFile( stream, ResourceFormatType.Yaml );

            res.Elements["key"]!.Comment.Should().BeEmpty( "blank line breaks the comment-to-key association" );
        }

        [Test]
        public void Load_DifferentValueStyles_AllParse()
        {
            var yaml = @"quoted: ""quoted value""
plain: plain value
single: 'single quoted'
block: |-
  line one
  line two";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( yaml ) );
            var res = new ResourceFile( stream, ResourceFormatType.Yaml );

            res.Elements["quoted"]!.Value.Should().Be( "quoted value" );
            res.Elements["plain"]!.Value.Should().Be( "plain value" );
            res.Elements["single"]!.Value.Should().Be( "single quoted" );
            res.Elements["block"]!.Value.Replace( "\r\n", "\n" ).Should().Be( "line one\nline two" );
        }

        [Test]
        public void Save_WithComment_EmitsHashLinesAboveKey()
        {
            var res = new ResourceFile( ResourceFormatType.Yaml );
            res.Elements.Add( "k1", "v1", comment: "important" );
            res.Elements.Add( "k2", "v2" );

            using var ms = new MemoryStream();
            res.Save( ms );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( "# important" );
            text.IndexOf( "# important" ).Should().BeLessThan( text.IndexOf( "k1:" ), "comment must precede its key" );
        }

        [Test]
        public void RoundTrip_PreservesCommentsValuesAndOrder()
        {
            var source = new ResourceFile( GetTestPath( "Resources.yaml" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.Yaml );
            source.Save( midPath, ResourceFormatType.Yaml );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }

        [Test]
        public void LoadRawElements_PreservesDuplicateKeys()
        {
            // duplicate keys: LoadResourceFile drops them (dictionary semantics),
            // LoadRawElements keeps both
            var yaml = @"key: first
key: second
other: value";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( yaml ) );
            var raw = ResourceFile.LoadRawElements( stream, ResourceFormatType.Yaml ).ToList();

            raw.Count.Should().Be( 3 );
            raw.Count( e => e.Key == "key" ).Should().Be( 2, "raw load preserves duplicate keys" );
        }

        [Test]
        public void LoadRawElements_PreservesOrder()
        {
            var yaml = @"zebra: z
apple: a
mango: m";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( yaml ) );
            var raw = ResourceFile.LoadRawElements( stream, ResourceFormatType.Yaml ).ToList();

            raw.Select( e => e.Key ).Should().Equal( "zebra", "apple", "mango" );
        }
    }
}
