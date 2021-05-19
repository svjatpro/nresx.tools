using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class SaveNewResourceFileTests : TestBase
    {
        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task SaveNewFile( ResourceFormatType targetType )
        {
            var path = GetOutputPath( UniqueKey(), targetType );

            var res = new ResourceFile( targetType );
            AddExampleElements( res );

            res.Save( path );

            var saved = new ResourceFile( path );
            saved.FileFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task SaveNewFileAs( ResourceFormatType targetType )
        {
            var path = GetOutputPath( UniqueKey(), targetType );

            var res = new ResourceFile();
            AddExampleElements( res );

            res.Save( path, targetType );

            var saved = new ResourceFile( path );
            saved.FileFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task SaveNewStream( ResourceFormatType targetType )
        {
            var res = new ResourceFile( targetType );
            AddExampleElements( res );

            var ms = new MemoryStream();
            res.Save( ms );

            var saved = new ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.FileFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task SaveNewStreamAs( ResourceFormatType targetType )
        {
            var res = new ResourceFile();
            AddExampleElements( res );

            var ms = new MemoryStream();
            res.Save( ms, targetType );

            var saved = new ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.FileFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task SaveToStream( ResourceFormatType targetType )
        {
            var res = new ResourceFile( targetType );
            AddExampleElements( res );

            var ms = res.SaveToStream();

            var saved = new ResourceFile( ms, targetType );
            saved.FileFormat.Should().Be( targetType );
            ValidateElements( saved );
        }
    }
}