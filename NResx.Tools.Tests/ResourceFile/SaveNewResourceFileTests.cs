using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NResx.Tools.Tests.ResourceFile
{
    [TestFixture]
    public class SaveNewResourceFileTests : TestBase
    {
        [TestCase( ResourceFormatType.Resx, @".resx" )]
        [TestCase( ResourceFormatType.Resw, @".resw" )]
        [TestCase( ResourceFormatType.Yml, @".yml" )]
        [TestCase( ResourceFormatType.Yaml, @".yaml" )]
        public async Task SaveNewFile( ResourceFormatType targetType, string extension )
        {
            var name = UniqueKey();
            var path = $"{name}{extension}";

            var res = new Tools.ResourceFile( targetType );
            AddExampleElements( res );

            res.Save( path );

            var saved = new Tools.ResourceFile( path );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx, @".resx" )]
        [TestCase( ResourceFormatType.Resw, @".resw" )]
        [TestCase( ResourceFormatType.Yml, @".yml" )]
        [TestCase( ResourceFormatType.Yaml, @".yaml" )]
        public async Task SaveNewFileAs( ResourceFormatType targetType, string extension )
        {
            var name = UniqueKey();
            var path = $"{name}{extension}";

            var res = new Tools.ResourceFile();
            AddExampleElements( res );

            res.Save( path, targetType );

            var saved = new Tools.ResourceFile( path );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx, @".resx" )]
        [TestCase( ResourceFormatType.Resw, @".resw" )]
        [TestCase( ResourceFormatType.Yml, @".yml" )]
        [TestCase( ResourceFormatType.Yaml, @".yaml" )]
        public async Task SaveNewStream( ResourceFormatType targetType, string extension )
        {
            var res = new Tools.ResourceFile( targetType );
            AddExampleElements( res );

            var ms = new MemoryStream();
            res.Save( ms );

            var saved = new Tools.ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx, @".resx" )]
        [TestCase( ResourceFormatType.Resw, @".resw" )]
        [TestCase( ResourceFormatType.Yml, @".yml" )]
        [TestCase( ResourceFormatType.Yaml, @".yaml" )]
        public async Task SaveNewStreamAs( ResourceFormatType targetType, string extension )
        {
            var res = new Tools.ResourceFile();
            AddExampleElements( res );

            var ms = new MemoryStream();
            res.Save( ms, targetType );

            var saved = new Tools.ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }

        [TestCase( ResourceFormatType.Resx, @".resx" )]
        [TestCase( ResourceFormatType.Resw, @".resw" )]
        [TestCase( ResourceFormatType.Yml, @".yml" )]
        [TestCase( ResourceFormatType.Yaml, @".yaml" )]
        public async Task SaveToStream( ResourceFormatType targetType, string extension )
        {
            var res = new Tools.ResourceFile( targetType );
            AddExampleElements( res );

            var ms = res.SaveToStream();

            var saved = new Tools.ResourceFile( ms, targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }
    }
}