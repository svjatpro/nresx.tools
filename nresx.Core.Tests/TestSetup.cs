using System.IO;
using NUnit.Framework;

namespace nresx.Core.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void GlobalSetUp()
        {
            TestBase.CleanOutputDir();
            EnsureXlsxFixture();
        }

        // RSX-138: XLSX is binary, so we can't author its fixture as a plain text file
        // in source control. Generate it from the canonical Resources.resx on each test
        // run — guarantees the fixture stays in sync with whatever Resources.resx says.
        private static void EnsureXlsxFixture()
        {
            var srcPath = Path.Combine( TestData.TestFileFolder, TestData.ExampleResourceFile );
            var dstPath = Path.Combine( TestData.TestFileFolder, "Resources.xlsx" );
            if ( !File.Exists( srcPath ) ) return;

            var src = new ResourceFile( srcPath );
            src.Save( dstPath, ResourceFormatType.Xlsx );
        }
    }
}