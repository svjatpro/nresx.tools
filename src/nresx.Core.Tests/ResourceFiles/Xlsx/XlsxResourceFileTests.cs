using System.IO;
using System.Linq;
using FluentAssertions;
using MiniExcelLibs;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Xlsx
{
    [TestFixture]
    public class XlsxResourceFileTests : TestBase
    {
        [Test]
        public void Load_FixtureHasExpectedShape()
        {
            // Fixture is regenerated each run from Resources.resx (TestSetup.GlobalSetUp)
            var res = new ResourceFile( GetTestPath( "Resources.xlsx" ) );

            res.FileFormat.Should().Be( ResourceFormatType.Xlsx );
            res.Elements.Should().NotBeEmpty();
        }

        [Test]
        public void Save_WritesKeyValueCommentColumns()
        {
            var res = new ResourceFile( ResourceFormatType.Xlsx );
            res.Elements.Add( "key1", "value1", comment: "translator hint" );
            res.Elements.Add( "key2", "value2" );

            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlsx );
            res.Save( dstPath );

            File.Exists( dstPath ).Should().BeTrue();

            // Verify column shape directly via MiniExcel - independent of our loader
            var rows = MiniExcel.Query( dstPath, useHeaderRow: true ).Cast<object>().ToList();
            rows.Should().HaveCount( 2 );
            var firstRow = (System.Collections.Generic.IDictionary<string, object?>) rows[0];
            firstRow.Keys.Should().BeEquivalentTo( "Key", "Value", "Comment" );
        }

        [Test]
        public void RoundTrip_KeysValuesCommentsMatch()
        {
            var src = new ResourceFile( ResourceFormatType.Xlsx );
            src.Elements.Add( "key1", "value1", comment: "comment1" );
            src.Elements.Add( "key2", "value2" );
            src.Elements.Add( "key3", "value3", comment: "comment3" );

            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlsx );
            src.Save( dstPath );

            var loaded = new ResourceFile( dstPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( src.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }

        [Test]
        public void Load_RecognizesCaseInsensitiveHeaders()
        {
            // Write a file using lowercase header names directly via MiniExcel, then load through our formatter
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlsx );
            var data = new[]
            {
                new { key = "k1", value = "v1", comment = "c1" },
                new { key = "k2", value = "v2", comment = "" },
            };
            MiniExcel.SaveAs( dstPath, data );

            var loaded = new ResourceFile( dstPath );

            loaded.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( new[] { ("k1", "v1"), ("k2", "v2") } );
            loaded.Elements["k1"]!.Comment.Should().Be( "c1" );
        }

        [Test]
        public void Load_SkipsFullyBlankRows()
        {
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlsx );
            var data = new[]
            {
                new { Key = "real1", Value = "v1", Comment = "" },
                new { Key = "",      Value = "",   Comment = "" },  // blank
                new { Key = "real2", Value = "v2", Comment = "" },
            };
            MiniExcel.SaveAs( dstPath, data );

            var loaded = new ResourceFile( dstPath, new ResourceFileOption { LoadMode = LoadMode.Lenient } );

            loaded.Elements.Select( e => e.Key ).Should().BeEquivalentTo( "real1", "real2" );
        }
    }
}
