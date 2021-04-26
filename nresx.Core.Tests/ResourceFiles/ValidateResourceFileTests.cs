using System.Threading.Tasks;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class ValidateResourceFileTests : TestBase
    {
        [Test]
        public async Task LoadWithDuplicatedEntries()
        {
            var res = new ResourceFile( GetTestPath( "Dulicated.resx" ) );

            //res.ResourceFormat.Should().Be( targetType );
            //ValidateElements( res );
        }
    }
}