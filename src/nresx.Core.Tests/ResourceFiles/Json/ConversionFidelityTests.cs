using System.Linq;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Json
{
    // RSX-253: a real i18next file must survive a json -> resx -> json roundtrip unchanged.
    [TestFixture]
    public class ConversionFidelityTests : TestBase
    {
        [Test]
        public void JsonToResxToJson_FlatFile_PreservesKeysValuesOrderAndShape()
        {
            // a flat i18next file, including a key literally named like element metadata ("comment")
            var src = GetOutputPath( UniqueKey(), ResourceFormatType.Json );
            var json = new ResourceFile( ResourceFormatType.Json );
            json.Elements.Add( "title", "Home" );
            json.Elements.Add( "save", "Save" );
            json.Elements.Add( "greeting", "Hello, {{name}}" );
            json.Elements.Add( "comment", "Leave a comment" );
            json.Save( src );

            // json -> resx -> json
            var resxPath = GetOutputPath( UniqueKey(), ResourceFormatType.Resx );
            new ResourceFile( src ).Save( resxPath, ResourceFormatType.Resx );
            var outPath = GetOutputPath( UniqueKey(), ResourceFormatType.Json );
            new ResourceFile( resxPath ).Save( outPath, ResourceFormatType.Json );

            var result = new ResourceFile( outPath );

            // key-set-identical, order-preserved, zero alien entries
            result.Elements.Select( el => (el.Key, el.Value) ).Should().Equal(
                ("title", "Home"),
                ("save", "Save"),
                ("greeting", "Hello, {{name}}"),
                ("comment", "Leave a comment") );

            // shape-identical: flat key:value, not "key": { "value": ... }
            result.ElementHasComment.Should().BeFalse();
        }

        [Test]
        public void SaveJson_ElementWithComment_UpgradesToKeyObjectSoCommentSurvives()
        {
            // no explicit shape: a commented element must be written as key:object so the comment
            // is not dropped (a flat key:value layout cannot carry it).
            var src = GetOutputPath( UniqueKey(), ResourceFormatType.Json );
            var json = new ResourceFile( ResourceFormatType.Json );
            json.Elements.Add( "welcome", "Welcome", "shown on the landing page" );
            json.Save( src );

            var result = new ResourceFile( src );

            result.ElementHasComment.Should().BeTrue();
            result.Elements.Single().Comment.Should().Be( "shown on the landing page" );
        }
    }
}
