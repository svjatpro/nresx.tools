using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Helpers;
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
            
            //var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            //res.Save( targetPath );

            //res = new ResourceFile( targetPath );

            res.FileName.Should().Be( Path.GetFileName( resPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( resPath ) );
            res.FileFormat.Should().Be( ResourceFormatType.Json );
            res.ElementHasKey.Should().BeTrue();
            //res.ElementHasComment.Should().BeFalse();

            //ValidateElements( res, ResourceFormatType.Json );
            res.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( GetExampleResourceFile().Elements.Select( el => (el.Key, el.Value) ) );
        }

    }
}