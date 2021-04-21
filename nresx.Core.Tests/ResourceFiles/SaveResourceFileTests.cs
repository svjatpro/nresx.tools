using System.IO;
using System.Threading.Tasks;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class SaveResourceFileTests : TestBase
    {
        [TestCase( @"Resources.resx" )]
        [TestCase( @"Resources.resw" )]
        [TestCase( @"Resources.yml" )]
        [TestCase( @"Resources.yaml" )]
        public async Task SaveAsFileInAnotherPath( string sourcePath )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var newPath = GetOutputPath( UniqueKey(), source.ResourceFormat );
            source.Save( newPath );

            var saved = new ResourceFile( newPath );
            ValidateElements( saved );
        }

        [TestCase( @"Resources.yml",  ResourceFormatType.Resx )]
        [TestCase( @"Resources.yaml", ResourceFormatType.Resw )]
        [TestCase( @"Resources.resx", ResourceFormatType.Yml )]
        [TestCase( @"Resources.resw", ResourceFormatType.Yaml )]
        public async Task SaveAsFile( string sourcePath, ResourceFormatType targetType )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var targetPath = GetOutputPath( UniqueKey(), targetType ); ;
            source.Save( targetPath, targetType );

            var saved = new ResourceFile( targetPath );
            ValidateElements( saved );
        }

        [TestCase( @"Resources.yml",  ResourceFormatType.Resx )]
        [TestCase( @"Resources.yaml", ResourceFormatType.Resw )]
        [TestCase( @"Resources.resx", ResourceFormatType.Yml )]
        [TestCase( @"Resources.resw", ResourceFormatType.Yaml )]
        public async Task SaveAsStream( string sourcePath, ResourceFormatType targetType )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var ms = new MemoryStream();
            source.Save( ms, targetType );

            var saved = new ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            ValidateElements( saved );
        }
    }
}