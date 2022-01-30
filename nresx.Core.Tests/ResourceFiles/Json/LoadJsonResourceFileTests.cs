using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Formatters;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Json
{
    [TestFixture]
    public class LoadJsonResourceFileTests : TestBase
    {
        [TestCase( @"json\plain.json", false )]
        [TestCase( @"json\plain_key_object.json", true )]
        [TestCase( @"json\plain_object.json", true )]
        [TestCase( @"json\struct_plain.json", false )]
        [TestCase( @"json\struct_plain_key_object.json", true )]
        [TestCase( @"json\struct_plain_object.json", true )]
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
            //res.Elements.Select( el => (el.Key, el.Value) ).Should()
            //    .BeEquivalentTo( GetExampleResourceFile().Elements.Select( el => (el.Key, el.Value) ) );
        }

        [Test]
        public async Task ParsePropertyNames()
        {
            var res = new ResourceFile( GetTestPath( @"json\struct_plain_object.json" ) );
            res.Elements
                .Select( el => el as ResourceElementJson )
                .Select( el => (key: el.KeyProperyName, val: el.ValueProperyName, comment: el.CommentProperyName) )
                .Should().BeEquivalentTo(
                    (key: "id", val: "text", comment: "comment"),
                    (key: "key", val: "value", comment: "description"),
                    (key: "name", val: "content", comment: "comment") );
        }

        [TestCase( @"json\plain.json", JsonElementType.KeyValue, "" )]
        [TestCase( @"json\plain_key_object.json", JsonElementType.KeyObject, "" )]
        [TestCase( @"json\plain_object.json", JsonElementType.Object, "strings" )]
        [TestCase( @"json\struct_plain.json", JsonElementType.KeyValue, "parent.middle" )]
        [TestCase( @"json\struct_plain_key_object.json", JsonElementType.KeyObject, "parent.middle" )]
        [TestCase( @"json\struct_plain_object.json", JsonElementType.Object, "parent.middle.strings" )]
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