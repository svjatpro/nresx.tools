using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Core;
using nresx.Core.Formatters;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Json
{
    [TestFixture]
    public class LoadJsonResourceFileTests : TestBase
    {
        [TestCase( @"json/plain.json", false )]
        [TestCase( @"json/plain_key_object.json", true )]
        [TestCase( @"json/plain_object.json", true )]
        [TestCase( @"json/struct_plain.json", false )]
        [TestCase( @"json/struct_plain_key_object.json", true )]
        [TestCase( @"json/struct_plain_object.json", true )]
        public async Task ParsePlainJson( string path, bool hasComments )
        {
            var resPath = GetTestPath( path );
            var res = new ResourceFile( resPath );
            
            res.FileName.Should().Be( Path.GetFileName( resPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( resPath ) );
            res.FileFormat.Should().Be( ResourceFormatType.Json );
            res.ElementHasKey.Should().BeTrue();
            res.ElementHasComment.Should().Be( hasComments );

            ValidateElements( res );
        }

        // RSX-251: nested/mixed i18next-shaped files. Keys from different subtrees are
        // qualified with the path relative to the common prefix (dot convention), plain
        // keys named like element metadata ("comment") stay elements of the container,
        // and the strict default load no longer throws false-duplicate errors.
        [Test]
        public async Task ParseNestedMixedJson()
        {
            var res = new ResourceFile( GetTestPath( @"json/i18next.json" ) );

            var keys = res.Elements.Select( el => el.Key ).ToList();
            keys.Should().OnlyHaveUniqueItems();
            keys.Should().BeEquivalentTo(
                "projectName", "comment", "record_one", "record_other",
                "statusSection.title", "statusSection.description",
                "actionsSection.title", "actionsSection.confirm.title", "actionsSection.confirm.message" );

            res.Elements.First( el => el.Key == "comment" ).Value
                .Should().Be( "A plain key that happens to be named like element metadata" );
            res.Elements.First( el => el.Key == "record_one" ).Value.Should().Be( "{{count}} record" );
        }

        // RSX-264: a truncated / malformed json must not hang the parser (the object/array loops
        // used to spin forever at EOF) nor throw an InvalidCastException - it loads leniently so
        // `validate` can report the file instead of crashing the whole run.
        [TestCase( "{ \"a\": \"1\", \"b\": " )]      // truncated object
        [TestCase( "{ \"list\": [ \"x\", \"y\" " )]  // truncated array
        [TestCase( "{ \"a\": { \"b\": " )]           // truncated nested object
        public void ParseTruncatedJsonDoesNotHangOrThrow( string content )
        {
            var path = Path.Combine( Path.GetTempPath(), $"nresx_trunc_{System.Guid.NewGuid():N}.json" );
            File.WriteAllText( path, content );
            try
            {
                var finished = Task.Run( () => { _ = new ResourceFile( path ); } )
                    .Wait( System.TimeSpan.FromSeconds( 10 ) );
                finished.Should().BeTrue( "a truncated json must load without hanging" );
            }
            finally
            {
                File.Delete( path );
            }
        }

        [Test]
        public async Task ParsePropertyNames()
        {
            var res = new ResourceFile( GetTestPath( @"json/struct_plain_object.json" ) );
            res.Elements
                .Select( el => el as ResourceElementJson )
                .Select( el => (key: el.KeyPropertyName, val: el.ValuePropertyName, comment: el.CommentPropertyName) )
                .Should().BeEquivalentTo( [
                    (key: "id", val: "text", comment: "comment"),
                    (key: "key", val: "value", comment: "description"),
                    (key: "name", val: "content", comment: "comment")] );
        }

        [TestCase( @"json/plain.json", JsonElementType.KeyValue, "" )]
        [TestCase( @"json/plain_key_object.json", JsonElementType.KeyObject, "" )]
        [TestCase( @"json/plain_object.json", JsonElementType.Object, "strings" )]
        [TestCase( @"json/struct_plain.json", JsonElementType.KeyValue, "parent.middle" )]
        [TestCase( @"json/struct_plain_key_object.json", JsonElementType.KeyObject, "parent.middle" )]
        [TestCase( @"json/struct_plain_object.json", JsonElementType.Object, "parent.middle.strings" )]
        public async Task ParseElementMetadata( string resourcePath, JsonElementType elementType, string elPath )
        {
            var res = new ResourceFile( GetTestPath( resourcePath ) );
            res.Elements
                .Select( el => el as ResourceElementJson )
                .All( el => el?.Path == elPath && el?.ElementType == elementType )
                .Should().BeTrue();
        }
    }
}