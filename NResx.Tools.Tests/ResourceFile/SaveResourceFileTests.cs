using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NResx.Tools.Tests.ResourceFile
{
    [TestFixture]
    public class SaveResourceFileTests : TestBase
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
            
            res.Save( path, targetType );

            var saved = new Tools.ResourceFile( path );
            saved.ResourceFormat.Should().Be( targetType );
            ValidateElements( saved );
        }
    }
}