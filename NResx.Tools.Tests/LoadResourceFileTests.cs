using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NResx.Tools.Tests
{
    [TestFixture]
    public class LoadResourceFileTests
    {
        [TestCase( @"Files\Resources.resx", ResourceFormatType.Resx )]
        [TestCase( @"Files\Resources.resw", ResourceFormatType.Resw )]
        public async Task LoadFromPathResw( string path, ResourceFormatType targetType )
        {
            var res = new ResourceFile( path );

            res.ResourceFormat.Should().Be( targetType );

            var elements = res.Elements.ToList();

            elements
                .Select( e => ( key: e.Key, val: e.Value ) )
                .Should().BeEquivalentTo(
                    ( key: "Entry1.Text", val: "Value1" ),
                    ( key: "Entry2", val: "Value2" ),
                    ( key: "Entry3", val: "Value3" ) );
        }
    }
}