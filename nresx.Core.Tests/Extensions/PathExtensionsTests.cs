using System.Threading.Tasks;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.Core.Tests.Extensions
{
    [TestFixture]
    public class PathExtensionsTests
    {
        [TestCase( "dir1\\en\\resource.resx", ExpectedResult = "en" )]
        [TestCase( "dir1\\en-CA\\resource.resx", ExpectedResult = "en-CA" )]
        [TestCase( "dir1\\fr-CA\\resource.resx", ExpectedResult = "fr-CA" )]
        [TestCase( "dir1\\res.en-CA.resx", ExpectedResult = "en-CA" )]
        [TestCase( "dir1\\resource.resx", ExpectedResult = null )]
        public async Task<string> TryToGetCulture( string path )
        {
            if ( path.TryToGetCulture( out var culture ) )
                return culture.Name;
            return null;
        }
    }
}
