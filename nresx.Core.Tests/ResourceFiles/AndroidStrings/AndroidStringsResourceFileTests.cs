using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.AndroidStrings
{
    [TestFixture]
    public class AndroidStringsResourceFileTests : TestBase
    {
        [Test]
        public void LoadAndroidStrings_HasExpectedElements()
        {
            var res = new ResourceFile( GetTestPath( "Resources.xml" ) );

            res.FileFormat.Should().Be( ResourceFormatType.AndroidStrings );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry1.Text"]!.Value.Should().Be( "Value1" );
            res.Elements["Entry2"]!.Value.Should().Be( "Value2" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [Test]
        public void LoadAndroidStrings_PrecedingCommentAttachesToNextString()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<resources>
  <!-- first comment -->
  <string name=""a"">A</string>
  <string name=""b"">B</string>
  <!-- third comment -->
  <string name=""c"">C</string>
</resources>";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( xml ) );
            var res = new ResourceFile( stream, ResourceFormatType.AndroidStrings );

            res.Elements["a"]!.Comment.Should().Be( "first comment" );
            res.Elements["b"]!.Comment.Should().BeEmpty();
            res.Elements["c"]!.Comment.Should().Be( "third comment" );
        }

        [Test]
        public void LoadAndroidStrings_HandlesEscapeSequences()
        {
            var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<resources>
  <string name=""apostrophe"">It\'s mine</string>
  <string name=""backslash"">a\\b</string>
  <string name=""tab"">col1\tcol2</string>
</resources>";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( xml ) );
            var res = new ResourceFile( stream, ResourceFormatType.AndroidStrings );

            res.Elements["apostrophe"]!.Value.Should().Be( "It's mine" );
            res.Elements["backslash"]!.Value.Should().Be( @"a\b" );
            res.Elements["tab"]!.Value.Should().Be( "col1\tcol2" );
        }

        [Test]
        public void SaveAndroidStrings_EscapesSpecialChars()
        {
            var res = new ResourceFile( ResourceFormatType.AndroidStrings );
            res.Elements.Add( "apostrophe", "It's mine" );
            res.Elements.Add( "backslash", @"a\b" );

            using var ms = new MemoryStream();
            res.Save( ms );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            var apos = doc.Root!.Elements( "string" ).Single( e => e.Attribute( "name" )!.Value == "apostrophe" );
            var backslash = doc.Root!.Elements( "string" ).Single( e => e.Attribute( "name" )!.Value == "backslash" );

            // raw XML text (not parsed), so the literal escape sequences are visible
            apos.Value.Should().Be( @"It\'s mine" );
            backslash.Value.Should().Be( @"a\\b" );
        }

        [Test]
        public void RoundTrip_KeysAndValuesMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.xml" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.AndroidStrings );
            source.Save( midPath, ResourceFormatType.AndroidStrings );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value) ) );
        }

        [Test]
        public void LoadAndroidStrings_NonResourcesRoot_ReturnsEmpty()
        {
            // Defensive: if a .xml file isn't actually Android strings.xml, load gracefully empty
            var xml = @"<?xml version=""1.0""?><config><setting/></config>";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( xml ) );
            var res = new ResourceFile( stream, ResourceFormatType.AndroidStrings );

            res.Elements.Should().BeEmpty();
        }
    }
}
