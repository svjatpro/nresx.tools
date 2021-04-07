using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class LoadResourceFileTests : TestBase
    {
        [TestCase( @"Resources.resx", ResourceFormatType.Resx )]
        [TestCase( @"Resources.resw", ResourceFormatType.Resw )]
        [TestCase( @"Resources.yml", ResourceFormatType.Yml )]
        [TestCase( @"Resources.yaml", ResourceFormatType.Yaml )]
        public async Task LoadFromPath( string path, ResourceFormatType targetType )
        {
            var res = new ResourceFile( GetTestPath( path ) );

            res.ResourceFormat.Should().Be( targetType );
            ValidateElements( res );
        }

        [TestCase( @"Resources.resx", ResourceFormatType.Resx )]
        [TestCase( @"Resources.resw", ResourceFormatType.Resw )]
        [TestCase( @"Resources.yml", ResourceFormatType.Yml )]
        [TestCase( @"Resources.yaml", ResourceFormatType.Yaml )]
        public async Task LoadFromStream( string path, ResourceFormatType targetType )
        {
            await using var stream = new FileStream( GetTestPath( path ), FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var res = new ResourceFile( stream );

            res.ResourceFormat.Should().Be( targetType );
            ValidateElements( res );
        }
    }
}