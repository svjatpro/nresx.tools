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
        public static readonly string ProjectsFolder = ".test_projects";

        public static readonly string DryRunOption = " --dry-run";
        public static readonly string RecursiveOption = " --recursive";
        public static readonly string RecursiveShortOption = " -r";

        //private static ResourceFormatType[] ResourceTypes = Enum.GetValues<ResourceFormatType>().Where( t => t != ResourceFormatType.NA ).ToArray();
        private static readonly Random FormatTypeRandom = new( (int) DateTime.Now.Ticks );
        private static readonly ResourceFormatType[] ResourceTypes =
        {
            ResourceFormatType.Resx,
            ResourceFormatType.Resw,
            ResourceFormatType.Yaml,
            ResourceFormatType.Yml,
            ResourceFormatType.Po,
        };

        public static IEnumerable ResourceFormats
        {
            get
            {
                yield return new TestCaseData( ResourceFormatType.Resx );
                yield return new TestCaseData( ResourceFormatType.Resw );
                yield return new TestCaseData( ResourceFormatType.Yaml );
                yield return new TestCaseData( ResourceFormatType.Yml );
                yield return new TestCaseData( ResourceFormatType.Po );
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
                yield return new TestCaseData( "Resources.po" );
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
        
        public static ResourceFormatType GetRandomType()
        {
            return ResourceTypes[FormatTypeRandom.Next( 0, ResourceTypes.Length - 1 )];
        }
    }
}