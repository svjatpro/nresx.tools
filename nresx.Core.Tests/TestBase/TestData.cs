using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        //private static readonly Random FormatTypeRandom = new( (int) DateTime.Now.Ticks );
        private static readonly Dictionary<ResourceFormatType, bool> ResourceTypes = new()
        {
            { ResourceFormatType.Resx, true }, // type, HasKey
            { ResourceFormatType.Resw, true },
            { ResourceFormatType.Yaml, true },
            { ResourceFormatType.Yml, true },
            { ResourceFormatType.Po, true },
            { ResourceFormatType.PlainText, false },
            { ResourceFormatType.Json, true }
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
                yield return new TestCaseData( ResourceFormatType.PlainText );
                yield return new TestCaseData( ResourceFormatType.Json );
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
                yield return new TestCaseData( "Resources.txt" );
                yield return new TestCaseData( "Resources.json" );
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
        
        public static ResourceFormatType GetRandomType( CommandRunOptions options = null )
        {
            var types = 
                ( options ?? new CommandRunOptions() ).SkipFilesWithoutKey ?
                ResourceTypes.Where( t => t.Value ).Select( t => t.Key ).ToArray() :
                ResourceTypes.Keys.ToArray();
            return types[new Random( (int) DateTime.Now.Ticks ).Next( 0, types.Length - 1 )];
        }
    }
}