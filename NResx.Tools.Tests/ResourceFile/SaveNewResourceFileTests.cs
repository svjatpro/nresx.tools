using System.IO;
using System.Threading.Tasks;
using Aspose.Cells;
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
            var example = GetExampleResourceFile();
            var name = UniqueKey();
            var path = $"{name}{extension}";

            var res = new Tools.ResourceFile( targetType );
            foreach ( var el in example.Elements )
                res.AddElement( el.Key, el.Value, el.Comment );

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
            var example = GetExampleResourceFile();
            var name = UniqueKey();
            var path = $"{name}{extension}";

            var res = new Tools.ResourceFile();
            foreach ( var el in example.Elements )
                res.AddElement( el.Key, el.Value, el.Comment );
            
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
            var example = GetExampleResourceFile();

            var res = new Tools.ResourceFile( targetType );
            foreach ( var el in example.Elements )
                res.AddElement( el.Key, el.Value, el.Comment );

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
        public async Task SaveNewSteamAs( ResourceFormatType targetType, string extension )
        {
            var example = GetExampleResourceFile();
            var name = UniqueKey();
            var path = $"{name}{extension}";

            var res = new Tools.ResourceFile();
            foreach ( var el in example.Elements )
                res.AddElement( el.Key, el.Value, el.Comment );

            var ms = new MemoryStream();
            res.Save( ms, targetType );

            var saved = new Tools.ResourceFile( new MemoryStream( ms.GetBuffer() ), targetType );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }



        // ----- new file

        //// (path), full path

        // (stream) // for new file

        //// (path, format)
        // (stream, format)

        // SaveToStream()


        // --------
        // (path), just new directory

        // (stream) // for new file

        // (path, format)
        // (stream, format)

        // SaveToStream()
    }
}