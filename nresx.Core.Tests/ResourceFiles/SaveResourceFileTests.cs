using System.IO;
using System.Threading.Tasks;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class SaveResourceFileTests : TestBase
    {
        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task SaveAsFileInAnotherPath( string sourcePath )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var newPath = GetOutputPath( UniqueKey(), source.FileFormat );
            source.Save( newPath );

            var saved = new ResourceFile( newPath );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task SaveAsFile( string sourcePath )
        {
            ResourceFormatHelper.DetectFormatByExtension( sourcePath, out var targetType );
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var targetPath = GetOutputPath( UniqueKey(), targetType ); ;
            source.Save( targetPath, targetType );

            var saved = new ResourceFile( targetPath );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task SaveAsStream( string sourcePath )
        {
            ResourceFormatHelper.DetectFormatByExtension( sourcePath, out var targetType );
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var ms = new MemoryStream();
            source.Save( ms, targetType );

            var saved = new ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            ValidateElements( saved );
        }
    }
}