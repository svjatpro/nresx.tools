using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace NResx.Tools.Tests.ResourceFile
{
    [TestFixture]
    public class CreateResourceFileTests : TestBase
    {
        [TestCase( ResourceFormatType.Resx )]
        [TestCase( ResourceFormatType.Resw )]
        [TestCase( ResourceFormatType.Yml )]
        [TestCase( ResourceFormatType.Yaml )]
        public async Task CreateResourceFile( ResourceFormatType targetType )
        {
            var example = GetExampleResourceFile();
            
            var res = new Tools.ResourceFile( targetType );
            foreach ( var el in example.Elements )
                res.AddElement( el.Key, el.Value, el.Comment );

            res.ResourceFormat.Should().Be( targetType );
            ValidateElements( res );
        }
    }
}