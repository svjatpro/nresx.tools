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
        [TestCase( @"Resources.resx" )]
        [TestCase( @"Resources.resw" )]
        [TestCase( @"Resources.yml" )]
        [TestCase( @"Resources.yaml" )]
        public async Task ParsedResourceFileShouldContainsFileNameAndPath( string path )
        {
            var res = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), res.ResourceFormat );
            res.Save( targetPath );

            res = new ResourceFile( targetPath );

            res.Name.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
        }

        [TestCase( @"Resources.resx" )]
        [TestCase( @"Resources.resw" )]
        [TestCase( @"Resources.yml" )]
        [TestCase( @"Resources.yaml" )]
        public async Task StreamParsedResourceFileShouldContainsFileNameAndPath( string path )
        {
            var res = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), res.ResourceFormat );
            res.Save( targetPath );

            await using var stream = new FileStream( targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            res = new ResourceFile( stream );

            res.Name.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
        }

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