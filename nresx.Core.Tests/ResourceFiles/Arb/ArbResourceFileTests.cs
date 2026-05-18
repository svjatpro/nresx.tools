using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Arb
{
    [TestFixture]
    public class ArbResourceFileTests : TestBase
    {
        [Test]
        public void Load_HasExpectedElements()
        {
            var res = new ResourceFile( GetTestPath( "Resources.arb" ) );

            res.FileFormat.Should().Be( ResourceFormatType.Arb );
            res.Elements.Select( el => el.Key ).Should()
                .BeEquivalentTo( "Entry1.Text", "Entry2", "Entry3" );
            res.Elements["Entry2"]!.Value.Should().Be( "Value2" );
            res.Elements["Entry2"]!.Comment.Should().Be( "Comment2" );
        }

        [Test]
        public void Load_IgnoresGlobalMetadata()
        {
            var text = @"{ ""@@locale"": ""en"", ""@@x-something"": ""drop"", ""greeting"": ""Hi"" }";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Arb );

            res.Elements.Select( e => e.Key ).Should().BeEquivalentTo( "greeting" );
        }

        [Test]
        public void Load_IgnoresMetadataWithoutDescription()
        {
            var text = @"{
              ""greeting"": ""Hi"",
              ""@greeting"": { ""type"": ""text"", ""placeholders"": {} }
            }";
            using var stream = new MemoryStream( Encoding.UTF8.GetBytes( text ) );
            var res = new ResourceFile( stream, ResourceFormatType.Arb );

            res.Elements["greeting"]!.Comment.Should().BeNullOrEmpty();
        }

        [Test]
        public void Save_EmitsMetadataBlockOnlyWhenCommentPresent()
        {
            var res = new ResourceFile( ResourceFormatType.Arb );
            res.Elements.Add( "withComment", "X", comment: "context for translators" );
            res.Elements.Add( "noComment", "Y" );

            using var ms = new MemoryStream();
            res.Save( ms, ResourceFormatType.Arb );
            ms.Position = 0;
            var text = new StreamReader( ms ).ReadToEnd();

            text.Should().Contain( "\"withComment\": \"X\"" );
            text.Should().Contain( "\"@withComment\":" );
            text.Should().Contain( "context for translators" );
            text.Should().NotContain( "\"@noComment\"" );
        }

        [Test]
        public void RoundTrip_KeysValuesCommentsMatch()
        {
            var source = new ResourceFile( GetTestPath( "Resources.arb" ) );
            var midPath = GetOutputPath( UniqueKey(), ResourceFormatType.Arb );
            source.Save( midPath, ResourceFormatType.Arb );

            var loaded = new ResourceFile( midPath );

            loaded.Elements.Select( el => (el.Key, el.Value, el.Comment) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value, el.Comment) ) );
        }
    }
}
