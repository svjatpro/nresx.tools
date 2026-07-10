using System.Threading.Tasks;
using nresx.Core.Extensions;
using NUnit.Framework;

namespace nresx.Core.Tests.Extensions
{
    [TestFixture]
    public class PathExtensionsTests
    {
        [TestCase( "dir1/en/resource.resx", ExpectedResult = "en" )]
        [TestCase( "dir1/en-CA/resource.resx", ExpectedResult = "en-CA" )]
        [TestCase( "dir1/fr-CA/resource.resx", ExpectedResult = "fr-CA" )]
        [TestCase( "dir1/res.en-CA.resx", ExpectedResult = "en-CA" )]
        [TestCase( "dir1/resource.resx", ExpectedResult = null )]
        // Windows also treats '\' as a separator; keep one guarded case so that coverage isn't lost.
        [TestCase( "dir1\\en\\resource.resx", ExpectedResult = "en", IncludePlatform = "Win" )]
        public async Task<string> TryToGetCulture( string path )
        {
            if ( path.TryToExtractCultureFromPath( out var culture ) )
                return culture.Name;
            return null;
        }

        [TestCase( "dir1/dir2/file1.po", ExpectedResult = true )]
        [TestCase( "file1.po", ExpectedResult = true )]

        [TestCase( null, ExpectedResult = false )]
        [TestCase( " ", ExpectedResult = false )]
        [TestCase( "*.po", ExpectedResult = false )]
        [TestCase( "dir1/dir2/*.po", ExpectedResult = false )]
        [TestCase( "dir1/dir2/file*.po", ExpectedResult = false )]
        [TestCase( "dir1/dir2/file1.*", ExpectedResult = false )]
        [TestCase( "dir1/dir2/file?.yml", ExpectedResult = false )]
        public async Task<bool> IsFileName( string path )
        {
            return path.IsRegularName();
        }
    }
}
