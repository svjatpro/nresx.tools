using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Extensions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.Core.Tests.Grouping
{
    [TestFixture]
    public class ResourceGroupValidateTests : TestBase
    {
        // Creates <OutputFolder>/<dir>/<key>_<locale>.resx for each locale (copied from the example),
        // in one shared folder so they form a single translation group. Paths use Path.Combine.
        private List<string> CreateGroup( string[] locales, out string key )
        {
            var dir = UniqueKey();
            key = UniqueKey();
            var paths = new List<string>();
            foreach ( var locale in locales )
            {
                var path = GetOutputPath( Path.Combine( dir, $"{key}_{locale}.resx" ) );
                TestHelper.CopyTemporaryFile( destPath: path );
                paths.Add( path );
            }
            return paths;
        }

        private static IReadOnlyList<ResourceValidationIssue> Validate( IEnumerable<string> paths, string baseLanguage = null )
        {
            var groups = ResourceGroup.Detect( paths, baseLanguage );
            return groups.SelectMany( g => g.Validate() ).ToList();
        }

        [Test]
        public void Validate_CleanGroup_HasNoIssues()
        {
            var files = CreateGroup( new[] { "en", "fr" }, out _ );

            // make fr values differ from en so nothing is flagged as NotTranslated
            var resFr = new ResourceFile( files[1] );
            for ( var i = 0; i < resFr.Elements.Count(); i++ )
                resFr.Elements[i]!.Value = UniqueKey();
            resFr.Save( files[1] );

            Validate( files ).Should().BeEmpty();
        }

        [Test]
        public void Validate_MissedElement_FlaggedInEachFileMissingTheKey()
        {
            var files = CreateGroup( new[] { "en", "fr" }, out _ );

            var resEn = new ResourceFile( files[0] );
            var enKey = UniqueKey();
            resEn.Elements.Add( enKey, UniqueKey() );
            resEn.Save( files[0] );

            var resFr = new ResourceFile( files[1] );
            for ( var i = 0; i < resFr.Elements.Count(); i++ )
                resFr.Elements[i]!.Value = UniqueKey();
            var frKey = UniqueKey();
            resFr.Elements.Add( frKey, UniqueKey() );
            resFr.Save( files[1] );

            var issues = Validate( files );

            issues.Should().Contain( i =>
                i.FilePath == new FileInfo( files[0] ).FullName &&
                i.Error.ErrorType == ResourceElementErrorType.MissedElement &&
                i.Error.ElementKey == frKey );
            issues.Should().Contain( i =>
                i.FilePath == new FileInfo( files[1] ).FullName &&
                i.Error.ErrorType == ResourceElementErrorType.MissedElement &&
                i.Error.ElementKey == enKey );
            issues.Should().HaveCount( 2 );
        }

        [Test]
        public void Validate_NotTranslated_FlaggedOnNonBaseFileMatchingBaseValue()
        {
            var files = CreateGroup( new[] { "en", "fr" }, out _ );

            // en is auto-picked as base; fr element[1] keeps the en value (untranslated), others differ
            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0]!.Value = UniqueKey();
            resFr.Elements[2]!.Value = UniqueKey();
            var untranslatedKey = resFr.Elements[1]!.Key;
            resFr.Save( files[1] );

            var issues = Validate( files );

            issues.Should().ContainSingle( i =>
                i.FilePath == new FileInfo( files[1] ).FullName &&
                i.Error.ErrorType == ResourceElementErrorType.NotTranslated &&
                i.Error.ElementKey == untranslatedKey );
        }

        [Test]
        public void Validate_ExplicitBaseLanguage_OverridesAutoPick()
        {
            var files = CreateGroup( new[] { "en", "fr" }, out _ );

            // make en differ from fr on [1] and [2]; keep [0] identical. With fr as base, en is flagged.
            var resEn = new ResourceFile( files[0] );
            resEn.Elements[1]!.Value = UniqueKey();
            resEn.Elements[2]!.Value = UniqueKey();
            var untranslatedKey = resEn.Elements[0]!.Key;
            resEn.Save( files[0] );

            var issues = Validate( files, baseLanguage: "fr" );

            issues.Should().ContainSingle( i =>
                i.FilePath == new FileInfo( files[0] ).FullName &&
                i.Error.ErrorType == ResourceElementErrorType.NotTranslated &&
                i.Error.ElementKey == untranslatedKey );
        }

        [Test]
        public void Validate_PerFileEmptyValue_FlaggedAsWarning()
        {
            var file = GetOutputPath( Path.Combine( UniqueKey(), $"{UniqueKey()}.resx" ) );
            TestHelper.CopyTemporaryFile( destPath: file );

            var res = new ResourceFile( file );
            res.Elements[1]!.Value = string.Empty;
            var emptyKey = res.Elements[1]!.Key;
            res.Save( file );

            var issues = Validate( new[] { file } );

            issues.Should().ContainSingle( i =>
                i.Error.ErrorType == ResourceElementErrorType.EmptyValue &&
                i.Error.ElementKey == emptyKey );
        }
    }
}
