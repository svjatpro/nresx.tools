using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Csv
{
    [TestFixture]
    public class CsvResourceFileTests : TestBase
    {
        [Test]
        public void Load_HasExpectedElements()
        {
            var res = new ResourceFile( GetTestPath( "Resources.csv" ) );

            res.FileFormat.Should().Be( ResourceFormatType.Csv );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [Test]
        public void Load_QuotedFieldsWithEmbeddedNewline_AreParsedAsSingleValue()
        {
            var res = new ResourceFile( GetTestPath( "Resources.csv" ) );

            res.Elements["Entry3"]!.Value.Should().Contain( "Value3" );
            res.Elements["Entry3"]!.Value.Should().Contain( "multiline" );
        }

        [Test]
        public void Load_NoHeaderRow_StillParsesData()
        {
            var text = "first,one,c1\nsecond,two,c2";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Csv );

            res.Elements.Select( e => e.Key ).Should().BeEquivalentTo( "first", "second" );
            res.Elements["first"]!.Value.Should().Be( "one" );
            res.Elements["second"]!.Comment.Should().Be( "c2" );
        }

        [Test]
        public void Load_EscapedDoubleQuote_IsUnescaped()
        {
            var text = "key,value,comment\nmsg,\"she said \"\"hi\"\"\",";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Csv );

            res.Elements["msg"]!.Value.Should().Be( "she said \"hi\"" );
        }

        [Test]
        public void Save_WritesHeaderRowAndQuotesWhenNeeded()
        {
            var res = new ResourceFile( ResourceFormatType.Csv );
            res.Elements.Add( "App.title", "MyApp", comment: "shown in title bar" );
            res.Elements.Add( "needsQuoting", "a, b, c" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Csv );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( "key,value,comment" );
            text.Should().Contain( "App.title,MyApp,shown in title bar" );
            text.Should().Contain( "\"a, b, c\"" );
        }

        [Test]
        public void Save_EmbeddedQuotesAreDoubled()
        {
            var res = new ResourceFile( ResourceFormatType.Csv );
            res.Elements.Add( "msg", "she said \"hi\"" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Csv );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( "\"she said \"\"hi\"\"\"" );
        }

        [Test]
        public void Tsv_LoadAndSave_UseTab()
        {
            var res = new ResourceFile( GetTestPath( "Resources.tsv" ) );
            res.FileFormat.Should().Be( ResourceFormatType.Tsv );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Tsv );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();
            text.Should().Contain( "key\tvalue\tcomment" );
            text.Should().Contain( "Entry2\tValue2\tComment2" );
        }

        [Test]
        public void RoundTrip_KeysValuesCommentsMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.csv" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.Csv );
            source.Save( midPath, ResourceFormatType.Csv );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }
    }
}
