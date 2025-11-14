using nresx.Tools;
using nresx.Tools.Helpers;
using nresx.Tools.ResourceFile;
using NUnit.Framework;
using System.IO;
using FluentAssertions;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class SaveResourceFileTests : TestBase
    {
        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public void SaveAsFileInAnotherPath( string sourcePath )
        {
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var newPath = GetOutputPath( UniqueKey(), source.FileFormat );
            source.Save( newPath );

            var saved = new ResourceFile( newPath );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public void SaveAsFile( string sourcePath )
        {
            ResourceFormatHelper.DetectFormatByExtension( sourcePath, out var targetType );
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var targetPath = GetOutputPath( UniqueKey(), targetType ); ;
            source.Save( targetPath, targetType );

            var saved = new ResourceFile( targetPath );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public void SaveAsStream( string sourcePath )
        {
            ResourceFormatHelper.DetectFormatByExtension( sourcePath, out var targetType );
            var source = new ResourceFile( GetTestPath( sourcePath ) );

            var ms = new MemoryStream();
            source.Save( ms, targetType );

            var saved = new ResourceFile( new MemoryStream( ms.ToArray() ), targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Po, "fr-FR" )]
        public void SaveCultureMetadata( ResourceFormatType format, string cultureCode )
        {
            var source = new ResourceFile( GetTestPath( TestData.ExampleResourceFile, format ) );
            var targetPath = GetOutputPath( TestData.UniqueKey(), format );

            source.Culture = new System.Globalization.CultureInfo( cultureCode );
            source.Save( targetPath );

            var saved = new ResourceFile( targetPath );
            saved.Culture.Name.Should().Be( cultureCode );
        }
    }
}