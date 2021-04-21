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
        [TestCase( ResourceFormatType.Resx )]
        [TestCase( ResourceFormatType.Resw )]
        [TestCase( ResourceFormatType.Yml )]
        [TestCase( ResourceFormatType.Yaml )]
        public async Task SaveNewFile( ResourceFormatType targetType )
        {
            var path = GetOutputPath( UniqueKey(), targetType );

            var res = new ResourceFile( targetType );
            AddExampleElements( res );

            res.Save( path );

            var saved = new ResourceFile( path );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx )]
        [TestCase( ResourceFormatType.Resw )]
        [TestCase( ResourceFormatType.Yml )]
        [TestCase( ResourceFormatType.Yaml )]
        public async Task SaveNewFileAs( ResourceFormatType targetType )
        {
            var path = GetOutputPath( UniqueKey(), targetType );

            var res = new ResourceFile();
            AddExampleElements( res );

            res.Save( path, targetType );

            var saved = new ResourceFile( path );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx )]
        [TestCase( ResourceFormatType.Resw )]
        [TestCase( ResourceFormatType.Yml )]
        [TestCase( ResourceFormatType.Yaml )]
        public async Task SaveNewStream( ResourceFormatType targetType )
        {
            var res = new ResourceFile( targetType );
            AddExampleElements( res );

            var ms = new MemoryStream();
            res.Save( ms );

            var saved = new ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx )]
        [TestCase( ResourceFormatType.Resw )]
        [TestCase( ResourceFormatType.Yml )]
        [TestCase( ResourceFormatType.Yaml )]
        public async Task SaveNewStreamAs( ResourceFormatType targetType )
        {
            var res = new ResourceFile();
            AddExampleElements( res );

            var ms = new MemoryStream();
            res.Save( ms, targetType );

            var saved = new ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx )]
        [TestCase( ResourceFormatType.Resw )]
        [TestCase( ResourceFormatType.Yml )]
        [TestCase( ResourceFormatType.Yaml )]
        public async Task SaveToStream( ResourceFormatType targetType )
        {
            var res = new ResourceFile( targetType );
            AddExampleElements( res );

            var ms = res.SaveToStream();

            var saved = new ResourceFile( ms, targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }
    }
}