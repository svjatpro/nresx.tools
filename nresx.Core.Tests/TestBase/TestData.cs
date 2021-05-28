using System;
using System.Collections;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests
{
    public class TestData
    {
        public static readonly string ExampleResourceFile = "Resources.resx";
        public static readonly string WrongFormatResourceFile = "Resources.non";

        public static readonly string OutputFolder = ".test_output";
        public static readonly string TestFileFolder = ".test_files";

        public static readonly string DryRunOption = " --dry-run";
        public static readonly string RecursiveOption = " --recursive";
        public static readonly string RecursiveShortOption = " -r";

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

        public static string UniqueKey( int length = 8 )
        {
            var key = Convert.ToBase64String( Guid.NewGuid().ToByteArray() )
                .Replace( "+", "" )
                .Replace( "/", "" )
                .Replace( "=", "" );
            return key.Substring( 0, Math.Min( length, key.Length ) );
        }
    }
}