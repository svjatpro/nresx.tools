using System.Threading.Tasks;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFormatHelpers
{
    [TestFixture]
    public class ResourceFormatHelperTests
    {
        [TestCase( @"fd\df\res.resx", ExpectedResult = ResourceFormatType.Resx )]
        [TestCase( @".reSw", ExpectedResult = ResourceFormatType.Resw )]
        [TestCase( @".yml", ExpectedResult = ResourceFormatType.Yml )]
        [TestCase( @".yaml", ExpectedResult = ResourceFormatType.Yaml )]
        public async Task<ResourceFormatType> DetectFormatByExtension( string path )
        {
            ResourceFormatHelper.DetectFormatByExtension( path, out var res );
            return res;
        }
    }
}
