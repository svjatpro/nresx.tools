using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using nresx.Core;
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

        private readonly struct TypeInfo
        {
            public bool HasKey { get; }
            public bool CanBeMangled { get; }
            public TypeInfo( bool hasKey, bool canBeMangled )
            {
                HasKey = hasKey;
                CanBeMangled = canBeMangled;
            }
        }

        // CanBeMangled = file survives TestHelper.ReplaceKey (byte-level text substitution).
        // .xlsx is a ZIP archive containing OpenXML SpreadsheetML XML files — text-level
        // substitution corrupts the ZIP central directory, so it's excluded from any pool
        // a mangling test draws from (see RequireMangleable below).
        //
        // Xlf/Xliff are intentionally still NOT in this random pool (a different concern):
        // adding them shifts the Random sequence and surfaces a pre-existing flaky
        // interaction in the CLI validate tests. Tracked separately as RSX-227.
        // Their behavior is covered by XliffResourceFileTests + XliffMangledFileTests
        // and by the explicit per-format ResourceFiles / ResourceFormats lists.
        private static readonly Dictionary<ResourceFormatType, TypeInfo> ResourceTypes = new()
        {
            { ResourceFormatType.Resx,      new TypeInfo( hasKey: true,  canBeMangled: true  ) },
            { ResourceFormatType.Resw,      new TypeInfo( hasKey: true,  canBeMangled: true  ) },
            { ResourceFormatType.Yaml,      new TypeInfo( hasKey: true,  canBeMangled: true  ) },
            { ResourceFormatType.Yml,       new TypeInfo( hasKey: true,  canBeMangled: true  ) },
            { ResourceFormatType.Po,        new TypeInfo( hasKey: true,  canBeMangled: true  ) },
            { ResourceFormatType.PlainText, new TypeInfo( hasKey: false, canBeMangled: true  ) },
            { ResourceFormatType.Json,      new TypeInfo( hasKey: true,  canBeMangled: true  ) },
            { ResourceFormatType.Xlsx,      new TypeInfo( hasKey: true,  canBeMangled: false ) }
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
                yield return new TestCaseData( ResourceFormatType.Xlf );
                yield return new TestCaseData( ResourceFormatType.Xliff );
                yield return new TestCaseData( ResourceFormatType.AndroidStrings );
                yield return new TestCaseData( ResourceFormatType.IosStrings );
                yield return new TestCaseData( ResourceFormatType.JavaProperties );
                yield return new TestCaseData( ResourceFormatType.Ini );
                yield return new TestCaseData( ResourceFormatType.Csv );
                yield return new TestCaseData( ResourceFormatType.Tsv );
                yield return new TestCaseData( ResourceFormatType.Arb );
                yield return new TestCaseData( ResourceFormatType.Xlsx );
            }
        }

        // Subset of ResourceFormats whose files tolerate TestHelper.ReplaceKey byte-level
        // text substitution. Use this for tests that mangle on-disk content (corrupt keys,
        // simulate broken files). Xlsx is excluded — it's a ZIP container of XML and
        // text-level substitution corrupts the central directory.
        public static IEnumerable ResourceFormatsMangleable
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
                yield return new TestCaseData( ResourceFormatType.Xlf );
                yield return new TestCaseData( ResourceFormatType.Xliff );
                yield return new TestCaseData( ResourceFormatType.AndroidStrings );
                yield return new TestCaseData( ResourceFormatType.IosStrings );
                yield return new TestCaseData( ResourceFormatType.JavaProperties );
                yield return new TestCaseData( ResourceFormatType.Ini );
                yield return new TestCaseData( ResourceFormatType.Csv );
                yield return new TestCaseData( ResourceFormatType.Tsv );
                yield return new TestCaseData( ResourceFormatType.Arb );
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
                yield return new TestCaseData( "Resources.xlf" );
                yield return new TestCaseData( "Resources.xliff" );
                yield return new TestCaseData( "Resources.xml" );
                yield return new TestCaseData( "Resources.strings" );
                yield return new TestCaseData( "Resources.properties" );
                yield return new TestCaseData( "Resources.ini" );
                yield return new TestCaseData( "Resources.csv" );
                yield return new TestCaseData( "Resources.tsv" );
                yield return new TestCaseData( "Resources.arb" );
                yield return new TestCaseData( "Resources.xlsx" );
            }
        }

        //public static IEnumerable Cultures
        //{
        //    get
        //    {
        //        yield return new TestCaseData( "en", new CultureInfo( "en" ) );
        //    }
        //}

        public static string UniqueKey( int length = 8 )
        {
            var key = Convert.ToBase64String( Guid.NewGuid().ToByteArray() )
                .Replace( "+", "" )
                .Replace( "/", "" )
                .Replace( "=", "" );
            return key[..Math.Min( length, key.Length )];
        }

        public static ResourceFormatType GetRandomType( CommandRunOptions options = null )
        {
            var opt = options ?? new CommandRunOptions();
            var types = ResourceTypes
                .Where( t => !opt.SkipFilesWithoutKey || t.Value.HasKey )
                .Where( t => !opt.RequireMangleable   || t.Value.CanBeMangled )
                .Select( t => t.Key )
                .ToArray();
            return types[new Random( (int) DateTime.Now.Ticks ).Next( 0, types.Length - 1 )];
        }
    }
}
