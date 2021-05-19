using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class CreateResourceFileTests : TestBase
    {
        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFormats ) )]
        public async Task CreateResourceFile( ResourceFormatType targetType )
        {
            var example = GetExampleResourceFile();
            
            var res = new ResourceFile( targetType );
            foreach ( var el in example.Elements )
                res.Elements.Add( el.Key, el.Value, el.Comment );

            res.FileFormat.Should().Be( targetType );
            ValidateElements( res );
        }
    }
}