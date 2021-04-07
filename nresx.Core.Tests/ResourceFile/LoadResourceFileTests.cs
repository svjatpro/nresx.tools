using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NResx.Tools.Tests.ResourceFile
{
    [TestFixture]
    public class LoadResourceFileTests : TestBase
    {
        [TestCase( @"Files\Resources.resx", ResourceFormatType.Resx )]
        [TestCase( @"Files\Resources.resw", ResourceFormatType.Resw )]
        [TestCase( @"Files\Resources.yml", ResourceFormatType.Yml )]
        [TestCase( @"Files\Resources.yaml", ResourceFormatType.Yaml )]
        public async Task LoadFromPath( string path, ResourceFormatType targetType )
        {
            var res = new Tools.ResourceFile( path );

            res.ResourceFormat.Should().Be( targetType );
            ValidateElements( res );
        }

        [TestCase( @"Files\Resources.resx", ResourceFormatType.Resx )]
        [TestCase( @"Files\Resources.resw", ResourceFormatType.Resw )]
        [TestCase( @"Files\Resources.yml", ResourceFormatType.Yml )]
        [TestCase( @"Files\Resources.yaml", ResourceFormatType.Yaml )]
        public async Task LoadFromStream( string path, ResourceFormatType targetType )
        {
            await using var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var res = new Tools.ResourceFile( stream );

            res.ResourceFormat.Should().Be( targetType );
            ValidateElements( res );
        }
    }
}