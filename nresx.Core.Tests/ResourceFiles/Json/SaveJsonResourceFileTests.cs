using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Formatters;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Json
{
    [TestFixture]
    public class SaveJsonResourceFileTests : TestBase
    {
        // structure

        [TestCase( @"json\plain.json" )]
        [TestCase( @"json\plain_key_object.json" )]
        [TestCase( @"json\plain_object.json" )]
        [TestCase( @"json\struct_plain.json" )]
        [TestCase( @"json\struct_plain_key_object.json" )]
        [TestCase( @"json\struct_plain_object.json")]
        public async Task SaveCustomPath( string path )
        {
            var resPath = GetTestPath( path );
            var res = new ResourceFile( resPath );
            var newName = GetOutputPath( UniqueKey(), ResourceFormatType.Json );
            var optionPath = "custom1.custom2";

            res.Save( newName, options : new ResourceFileOptionJson{ Path = optionPath } );

            res = new ResourceFile( newName );
            res.Elements.Should().AllBeEquivalentTo( 
                new ResourceElementJson{ Path = optionPath },
                opt => opt.Including( el => el.Path ));
        }

        [TestCase( @"json\plain.json" )]
        [TestCase( @"json\plain_key_object.json" )]
        [TestCase( @"json\plain_object.json" )]
        [TestCase( @"json\struct_plain.json" )]
        [TestCase( @"json\struct_plain_key_object.json" )]
        [TestCase( @"json\struct_plain_object.json" )]
        public async Task SaveCustomProperties( string path )
        {
            var resPath = GetTestPath( path );
            var res = new ResourceFile( resPath );
            var newName = GetOutputPath( UniqueKey(), ResourceFormatType.Json );
            var options = new ResourceFileOptionJson
            {
                KeyName = "customKey",
                ValueName = "customName",
                CommentName = "customComment"
            };

            res.Save( newName, options: options );

            res = new ResourceFile( newName, options );
            res.Elements.Should().AllBeEquivalentTo(
                new ResourceElementJson
                {
                    //KeyProperyName = options.KeyName,
                    ValueProperyName = options.ValueName,
                    CommentProperyName = options.CommentName
                },
                opt => opt
                    //.Including( el => el.KeyProperyName )
                    .Including( el => el.ValueProperyName )
                    .Including( el => el.CommentProperyName ) );
        }


        [TestCase( @"json\plain.json", JsonElementType.KeyObject )]
        [TestCase( @"json\plain_key_object.json", JsonElementType.KeyValue )]
        [TestCase( @"json\plain_object.json", JsonElementType.Object )]
        [TestCase( @"json\struct_plain.json", JsonElementType.KeyObject )]
        [TestCase( @"json\struct_plain_key_object.json", JsonElementType.KeyValue )]
        [TestCase( @"json\struct_plain_object.json", JsonElementType.Object )]
        public async Task SaveCustomStructure( string path, JsonElementType elementType )
        {
            var resPath = GetTestPath( path );
            var res = new ResourceFile( resPath );
            var newName = GetOutputPath( UniqueKey(), ResourceFormatType.Json );
            
            res.Save( newName, options: new ResourceFileOptionJson { ElementType = elementType } );

            res = new ResourceFile( newName );
            res.Elements.Should().AllBeEquivalentTo(
                new ResourceElementJson { ElementType = elementType },
                opt => opt.Including( el => el.ElementType ) );
        }
    }
}