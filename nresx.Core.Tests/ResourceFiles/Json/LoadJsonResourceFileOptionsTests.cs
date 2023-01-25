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
    public class LoadJsonResourceFileOptionsTests : TestBase
    {
        #region Options.Path

        [TestCase( @"json\plain.json" )]
        [TestCase( @"json\plain_key_object.json" )]
        [TestCase( @"json\plain_object.json" )]
        [TestCase( @"json\struct_plain.json" )]
        [TestCase( @"json\struct_plain_key_object.json" )]
        [TestCase( @"json\struct_plain_object.json" )]
        public async Task LoadWithOptionsPathEmpty( string resourcePath )
        {
            var res = new ResourceFile( GetTestPath( resourcePath ), new ResourceFileOptionJson { Path = "wrong_parent" } );
            res.Elements.Should().BeEmpty();
        }

        [TestCase(@"json\plain.json", "")]
        [TestCase(@"json\plain_key_object.json", "")]
        [TestCase(@"json\plain_object.json", "strings")]
        [TestCase(@"json\struct_plain.json", "parent.middle")]
        [TestCase(@"json\struct_plain_key_object.json", "parent.middle")]
        [TestCase(@"json\struct_plain_object.json", "parent.middle.strings")]
        public async Task LoadWithOptions( string resourcePath, string elPath )
        {
            var res = new ResourceFile( GetTestPath( resourcePath ), new ResourceFileOptionJson{ Path = elPath } );

            res.Elements
                .Select( el => el as ResourceElementJson )
                .All( el => el?.Path == elPath )
                .Should().BeTrue();
        }

        #endregion

        #region Options.KeyName

        [Test]
        public async Task ParseElementsMetadataEmptyKey()
        {
            var res = new ResourceFile( GetTestPath( @"json\struct_plain_object.json" ), new ResourceFileOptionJson() );
            res.Elements
                .Select( el => el as ResourceElementJson )
                .Select( el => (key: el.KeyProperyName, val: el.ValueProperyName, comment: el.CommentProperyName) )
                .Should().BeEquivalentTo(
                    (key: "id", val: "text", comment: "comment"),
                    (key: "key", val: "value", comment: "description"),
                    (key: "name", val: "content", comment: "comment") );
        }
        
        [TestCase( @"json\plain_object.json", "id", "Entry1.Text" )]
        [TestCase( @"json\struct_plain_object.json", "key", "Entry2" )]
        public async Task ParseElementMetadata( string resourcePath, string keyName, string keyValue )
        {
            var res = new ResourceFile( GetTestPath( resourcePath ), new ResourceFileOptionJson{ KeyName = keyName }  );
            res.Elements
                .Select( el => el as ResourceElementJson )
                .Select( el => (el.KeyProperyName, el.Key) )
                .Should().BeEquivalentTo( (keyName, keyValue) );
        }

        [Test]
        [Ignore("temporary disabled")]
        public async Task ParseElementMetadataCustomFields()
        {
            var res = new ResourceFile( GetTestPath( @"json\plain_object_custom.json" ),
                new ResourceFileOptionJson { KeyName = "id2", ValueName = "text_custom2" } );
            res.Elements
                .Select( el => el as ResourceElementJson )
                .Select( el => (el.Key, el.Value) )
                .Should().BeEquivalentTo( ("Entry2.Text", "Value2") );
        }

        #endregion
    }
}