using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Ini
{
    [TestFixture]
    public class IniResourceFileTests : TestBase
    {
        [Test]
        public void Load_HasExpectedElements()
        {
            var res = new ResourceFile( GetTestPath( "Resources.ini" ) );

            res.FileFormat.Should().Be( ResourceFormatType.Ini );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [Test]
        public void Load_SectionedKeys_PrefixedWithSection()
        {
            var text = "[General]\nname=app\n\n[Account]\nuser=alice";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Ini );

            res.Elements.Select( e => e.Key ).Should().BeEquivalentTo( "General.name", "Account.user" );
            res.Elements["General.name"]!.Value.Should().Be( "app" );
            res.Elements["Account.user"]!.Value.Should().Be( "alice" );
        }

        [Test]
        public void Load_AcceptsHashAndSemicolonComments()
        {
            var text = "; first\nfirst=1\n# second\nsecond=2";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Ini );

            res.Elements["first"]!.Comment.Should().Be( "first" );
            res.Elements["second"]!.Comment.Should().Be( "second" );
        }

        [Test]
        public void Load_SurroundingQuotes_AreStripped()
        {
            var text = "msg=\"hello world\"";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Ini );

            res.Elements["msg"]!.Value.Should().Be( "hello world" );
        }

        [Test]
        public void Save_WritesFlatKeysAndComments()
        {
            // Sections are honored on load (for reading existing files) but Save
            // emits flat `key=value` to keep round-trip clean — see formatter comment.
            var res = new ResourceFile( ResourceFormatType.Ini );
            res.Elements.Add( "App.title", "MyApp", comment: "shown in title bar" );
            res.Elements.Add( "App.version", "1.0" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Ini );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( "App.title=MyApp" );
            text.Should().Contain( "; shown in title bar" );
            text.Should().NotContain( "[App]" );
        }

        [Test]
        public void Save_EscapesNewlinesInValue()
        {
            var res = new ResourceFile( ResourceFormatType.Ini );
            res.Elements.Add( "multi", "line1\nline2" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Ini );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( @"multi=line1\nline2" );
        }

        [Test]
        public void RoundTrip_KeysValuesCommentsMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.ini" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.Ini );
            source.Save( midPath, ResourceFormatType.Ini );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }
    }
}
