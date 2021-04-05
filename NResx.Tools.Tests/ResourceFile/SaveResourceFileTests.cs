using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NResx.Tools.Tests.ResourceFile
{
    [TestFixture]
    public class SaveResourceFileTests : TestBase
    {
        [TestCase( @"Files\Resources.resx" )]
        [TestCase( @"Files\Resources.resw" )]
        [TestCase( @"Files\Resources.yml" )]
        [TestCase( @"Files\Resources.yaml" )]
        public async Task SaveAsFileInAnotherPath( string sourcePath )
        {
            var source = new Tools.ResourceFile( sourcePath );

            var newPath = $"{UniqueKey()}{Path.GetExtension( sourcePath )}";
            source.Save( newPath );

            var saved = new Tools.ResourceFile( newPath );
            ValidateElements( saved );
        }

        [TestCase( @"Files\Resources.yml",  @".resx", ResourceFormatType.Resx )]
        [TestCase( @"Files\Resources.yaml", @".resw", ResourceFormatType.Resw )]
        [TestCase( @"Files\Resources.resx", @".yml",  ResourceFormatType.Yml )]
        [TestCase( @"Files\Resources.resw", @".yaml", ResourceFormatType.Yaml )]
        public async Task SaveAsFile( string sourcePath, string targetExt, ResourceFormatType targetType )
        {
            var source = new Tools.ResourceFile( sourcePath );

            var targetPath = $"{UniqueKey()}{targetExt}";
            source.Save( targetPath, targetType );

            var saved = new Tools.ResourceFile( targetPath );
            ValidateElements( saved );
        }

        [TestCase( @"Files\Resources.yml",  @".resx", ResourceFormatType.Resx )]
        [TestCase( @"Files\Resources.yaml", @".resw", ResourceFormatType.Resw )]
        [TestCase( @"Files\Resources.resx", @".yml",  ResourceFormatType.Yml )]
        [TestCase( @"Files\Resources.resw", @".yaml", ResourceFormatType.Yaml )]
        public async Task SaveAsStream( string sourcePath, string targetExt, ResourceFormatType targetType )
        {
            var source = new Tools.ResourceFile( sourcePath );

            var ms = new MemoryStream();
            source.Save( ms, targetType );

            var saved = new Tools.ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            ValidateElements( saved );
        }
    }
}