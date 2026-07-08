using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Xliff
{
    [TestFixture]
    public class XliffResourceFileTests : TestBase
    {
        private static readonly XNamespace Ns = "urn:oasis:names:tc:xliff:document:1.2";

        [TestCase( "Resources.xlf" )]
        [TestCase( "Resources.xliff" )]
        public void LoadXliff_HasExpectedElements( string fileName )
        {
            var res = new ResourceFile( GetTestPath( fileName ) );

            res.FileFormat.Should().BeOneOf( ResourceFormatType.Xlf, ResourceFormatType.Xliff );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry1.Text"]!.Value.Should().Be( "Value1" );
            res.Elements["Entry2"]!.Value.Should().Be( "Value2" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [Test]
        public void LoadXliff_TargetMissing_FallsBackToSource()
        {
            // build an XLIFF in memory where target is empty
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xliff version=""1.2"" xmlns=""urn:oasis:names:tc:xliff:document:1.2"">
  <file source-language=""en"" target-language="""" datatype=""plaintext"" original=""m"">
    <body>
      <trans-unit id=""hello""><source>Hello</source></trans-unit>
      <trans-unit id=""empty-target""><source>Source</source><target></target></trans-unit>
      <trans-unit id=""translated""><source>Hi</source><target>Bonjour</target></trans-unit>
    </body>
  </file>
</xliff>";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( xml ) );
            var res = new ResourceFile( stream, ResourceFormatType.Xliff );

            res.Elements["hello"]!.Value.Should().Be( "Hello" );
            res.Elements["empty-target"]!.Value.Should().Be( "Source" );
            res.Elements["translated"]!.Value.Should().Be( "Bonjour" );
        }

        [Test]
        public void SaveXliff_WritesSourceAndTargetWithSameValue()
        {
            var res = new ResourceFile( ResourceFormatType.Xliff );
            res.Elements.Add( "Greeting", "Hello", comment: "Used on home page" );

            using var ms = new MemoryStream();
            res.Save( ms );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            var unit = doc.Root!.Descendants( Ns + "trans-unit" ).Single();
            unit.Attribute( "id" )!.Value.Should().Be( "Greeting" );
            unit.Element( Ns + "source" )!.Value.Should().Be( "Hello" );
            unit.Element( Ns + "target" )!.Value.Should().Be( "Hello" );
            unit.Element( Ns + "note" )!.Value.Should().Be( "Used on home page" );
        }

        [Test]
        public void SaveXliff_WritesTargetLanguageFromCulture()
        {
            var res = new ResourceFile( ResourceFormatType.Xliff )
            {
                Culture = new System.Globalization.CultureInfo( "fr-FR" )
            };
            res.Elements.Add( "k", "v" );

            using var ms = new MemoryStream();
            res.Save( ms );
            ms.Position = 0;

            var doc = XDocument.Load( ms );
            doc.Root!.Element( Ns + "file" )!.Attribute( "target-language" )!.Value
                .Should().Be( "fr-FR" );
        }

        [Test]
        public void RoundTrip_XlfToXliffAndBack_KeysAndValuesMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.xlf" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xliff );
            source.Save( midPath, ResourceFormatType.Xliff );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value) ) );
        }
    }
}
