using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    // RSX-117: resx-specific options for metadata that's currently hardcoded.
    [TestFixture]
    public class ResxOptionsTests : TestBase
    {
        [Test]
        public void Save_DefaultOptions_WritesCommentSchemaAndHeaders()
        {
            var res = GetExampleResourceFile();
            using var ms = new MemoryStream();
            res.Save( ms );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            var root = doc.Root!;

            root.Nodes().OfType<XComment>().Should().NotBeEmpty( "default writes the Microsoft commentary" );
            root.Elements().Any( e => e.Name.LocalName == "schema" ).Should().BeTrue( "default writes the XSD schema" );
            root.Elements( "resheader" ).Should().HaveCount( 4, "default writes the four standard resheaders" );
        }

        [Test]
        public void Save_OmitRootComment_ProducesNoTopLevelComment()
        {
            var res = GetExampleResourceFile();
            var opts = new ResourceFileOptionResx { WriteRootComment = false };

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Resx, opts );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            doc.Root!.Nodes().OfType<XComment>().Should().BeEmpty();
        }

        [Test]
        public void Save_OmitSchema_ProducesNoSchemaElement()
        {
            var res = GetExampleResourceFile();
            var opts = new ResourceFileOptionResx { WriteEmbeddedSchema = false };

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Resx, opts );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            doc.Root!.Elements().Any( e => e.Name.LocalName == "schema" ).Should().BeFalse();
        }

        [Test]
        public void Save_OmitStandardResHeaders_ProducesNoResheaders()
        {
            var res = GetExampleResourceFile();
            var opts = new ResourceFileOptionResx { WriteStandardResHeaders = false };

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Resx, opts );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            doc.Root!.Elements( "resheader" ).Should().BeEmpty();
        }

        [Test]
        public void Save_FullyMinified_StillReadable()
        {
            // Drop everything optional — file should still load back with same elements
            var res = GetExampleResourceFile();
            var opts = new ResourceFileOptionResx
            {
                WriteRootComment = false,
                WriteEmbeddedSchema = false,
                WriteStandardResHeaders = false,
            };

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Resx, opts );
            ms.Position = 0;

            var reloaded = new ResourceFile( ms, ResourceFormatType.Resx );

            reloaded.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( res.Elements.Select( el => (el.Key, el.Value) ) );
        }

        [Test]
        public void Save_MinifiedFile_IsSubstantiallySmaller()
        {
            var res = GetExampleResourceFile();

            using var defaultMs = new MemoryStream();
            res.Save( defaultMs );

            using var minMs = new MemoryStream();
            res.Save( minMs, ResourceFormatType.Resx, new ResourceFileOptionResx
            {
                WriteRootComment = false,
                WriteEmbeddedSchema = false,
                WriteStandardResHeaders = false,
            } );

            // Schema + comment are roughly ~3 KB combined; minified should be far smaller
            minMs.Length.Should().BeLessThan( defaultMs.Length / 2,
                "minified output should be at least half the size of the default" );
        }
    }
}
