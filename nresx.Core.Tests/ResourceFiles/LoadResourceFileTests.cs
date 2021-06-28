using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class LoadResourceFileTests : TestBase
    {
        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task ParsedResourceFileShouldContainsFileNameAndPath( string path )
        {
            var res = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            res.Save( targetPath );

            res = new ResourceFile( targetPath );

            res.FileName.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task StreamParsedResourceFileShouldContainsFileNameAndPath( string path )
        {
            var res = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            res.Save( targetPath );

            await using var stream = new FileStream( targetPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            res = new ResourceFile( stream );

            res.FileName.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task LoadFromPath( string path )
        {
            ResourceFormatHelper.DetectFormatByExtension( path, out var targetType );
            var res = new ResourceFile( GetTestPath( path ) );

            res.FileFormat.Should().Be( targetType );
            ValidateElements( res );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task LoadFromStream( string path )
        {
            ResourceFormatHelper.DetectFormatByExtension( path, out var targetType );
            await using var stream = new FileStream( GetTestPath( path ), FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var res = new ResourceFile( stream );

            res.FileFormat.Should().Be( targetType );
            ValidateElements( res );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task LoadRawElements( ResourceFormatType format )
        {
            var path = GetTestPath( "Duplicated", format );
            var elements = ResourceFile.LoadRawElements( path );

            elements.Should().HaveCount( 5 );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task LoadRawElementsByStream( ResourceFormatType format )
        {
            var path = GetTestPath( "Duplicated", format );
            await using var stream = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );

            var elements = ResourceFile.LoadRawElements( stream );

            elements.Should().HaveCount( 5 );
        }
    }
}