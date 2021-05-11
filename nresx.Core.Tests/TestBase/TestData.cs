using System.Collections;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests
{
    public class TestData
    {
        public static readonly string ExampleResourceFile = "Resources.resx";

        public static readonly string OutputFolder = ".test_output";
        public static readonly string TestFileFolder = ".test_files";

        public static IEnumerable ResourceFormats
        {
            get
            {
                yield return new TestCaseData( ResourceFormatType.Resx );
                yield return new TestCaseData( ResourceFormatType.Resw );
                yield return new TestCaseData( ResourceFormatType.Yaml );
                yield return new TestCaseData( ResourceFormatType.Yml );
            }
        }

        public static IEnumerable ResourceFiles
        {
            get
            {
                yield return new TestCaseData( "Resources.resx" );
                yield return new TestCaseData( "Resources.resw" );
                yield return new TestCaseData( "Resources.yaml" );
                yield return new TestCaseData( "Resources.yml" );
            }
        }
    }
}