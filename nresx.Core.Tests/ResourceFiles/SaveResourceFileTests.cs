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
            var source = new nresx.Tools.ResourceFile( GetTestPath( sourcePath ) );

            var newPath = $"{UniqueKey()}{Path.GetExtension( sourcePath )}";
            source.Save( newPath );

            var saved = new ResourceFile( newPath );
            ValidateElements( saved );
        }

        [TestCase( @"Resources.yml",  @".resx", ResourceFormatType.Resx )]
        [TestCase( @"Resources.yaml", @".resw", ResourceFormatType.Resw )]
        [TestCase( @"Resources.resx", @".yml",  ResourceFormatType.Yml )]
        [TestCase( @"Resources.resw", @".yaml", ResourceFormatType.Yaml )]
        public async Task SaveAsFile( string sourcePath, string targetExt, ResourceFormatType targetType )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var targetPath = $"{UniqueKey()}{targetExt}";
            source.Save( targetPath, targetType );

            var saved = new ResourceFile( targetPath );
            ValidateElements( saved );
        }

        [TestCase( @"Resources.yml",  @".resx", ResourceFormatType.Resx )]
        [TestCase( @"Resources.yaml", @".resw", ResourceFormatType.Resw )]
        [TestCase( @"Resources.resx", @".yml",  ResourceFormatType.Yml )]
        [TestCase( @"Resources.resw", @".yaml", ResourceFormatType.Yaml )]
        public async Task SaveAsStream( string sourcePath, string targetExt, ResourceFormatType targetType )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var ms = new MemoryStream();
            source.Save( ms, targetType );

            var saved = new nresx.Tools.ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            ValidateElements( saved );
        }
    }
}