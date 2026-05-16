using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.JavaProperties
{
    [TestFixture]
    public class JavaPropertiesResourceFileTests : TestBase
    {
        [Test]
        public void Load_HasExpectedElements()
        {
            var res = new ResourceFile( GetTestPath( "Resources.properties" ) );

            res.FileFormat.Should().Be( ResourceFormatType.JavaProperties );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [TestCase( "key=value" )]
        [TestCase( "key = value" )]
        [TestCase( "key:value" )]
        [TestCase( "key : value" )]
        [TestCase( "key value" )]
        [TestCase( "key\tvalue" )]
        public void Load_AcceptsDifferentSeparators( string line )
        {
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( line ) );
            var res = new ResourceFile( stream, ResourceFormatType.JavaProperties );

            res.Elements["key"]!.Value.Should().Be( "value" );
        }

        [Test]
        public void Load_LineContinuation_JoinsLines()
        {
            // Trailing `\` continues the value onto the next line
            var text = "key=line one\\\n  line two";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.JavaProperties );

            res.Elements["key"]!.Value.Should().NotBeNullOrEmpty();
            res.Elements["key"]!.Value.Should().Contain( "line one" );
        }

        [Test]
        public void Load_BangComment_AlsoAttaches()
        {
            // `!` is a valid comment prefix in .properties, alongside `#`
            var text = "! a bang comment\nkey=value";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.JavaProperties );

            res.Elements["key"]!.Comment.Should().Be( "a bang comment" );
        }

        [Test]
        public void Load_UnicodeEscape_Decoded()
        {
            var text = @"name=étoile";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.JavaProperties );

            res.Elements["name"]!.Value.Should().Be( "étoile" );
        }

        [Test]
        public void Save_EscapesSpecialChars()
        {
            var res = new ResourceFile( ResourceFormatType.JavaProperties );
            res.Elements.Add( "key with spaces", "v = w" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.JavaProperties );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            // spaces in key get escaped; `=` in value is fine (only matters in key)
            text.Should().Contain( @"key\ with\ spaces" );
        }

        [Test]
        public void RoundTrip_KeysValuesCommentsMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.properties" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.JavaProperties );
            source.Save( midPath, ResourceFormatType.JavaProperties );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }
    }
}
