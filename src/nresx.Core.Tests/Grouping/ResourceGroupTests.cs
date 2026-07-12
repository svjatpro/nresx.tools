using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.Core.Tests.Grouping
{
    [TestFixture]
    public class ResourceGroupTests : TestBase
    {
        // Copies the example resx to <OutputFolder>/<relativePath> (creating directories) and
        // returns the destination path. relativePath uses forward slashes for cross-platform safety.
        private string CreateResx( string relativePath )
        {
            var dest = GetOutputPath( relativePath.Replace( '/', Path.DirectorySeparatorChar ) );
            TestHelper.CopyTemporaryFile( destPath: dest );
            return dest;
        }

        [Test]
        public void Detect_SameFolder_GroupsByFormatAndCulture()
        {
            var dir = UniqueKey();
            var key = UniqueKey();
            var en = CreateResx( $"{dir}/{key}_en.resx" );
            var fr = CreateResx( $"{dir}/{key}_fr.resx" );

            var groups = ResourceGroup.Detect( new[] { en, fr } );

            groups.Should().HaveCount( 1 );
            groups[0].Files.Select( f => f.FileName )
                .Should().BeEquivalentTo( new FileInfo( en ).Name, new FileInfo( fr ).Name );
        }

        [Test]
        public void Detect_CultureSpecificFolders_GroupsByGrandparent()
        {
            var dir = UniqueKey();
            var key = UniqueKey();
            var en = CreateResx( $"{dir}/en/{key}_en.resx" );
            var fr = CreateResx( $"{dir}/fr/{key}_fr.resx" );

            var groups = ResourceGroup.Detect( new[] { en, fr } );

            groups.Should().HaveCount( 1 );
            groups[0].Files.Should().HaveCount( 2 );
        }

        [Test]
        public void Detect_NamespaceLayout_GroupsByNameAcrossCultureDirs()
        {
            // locales/<lang>/<ns>.resx: culture comes from the folder; same namespace across
            // languages pairs up, different namespaces stay in separate groups.
            var root = UniqueKey();
            var enCommon = CreateResx( $"{root}/locales/en/common.resx" );
            var ukCommon = CreateResx( $"{root}/locales/uk/common.resx" );
            var enAccounting = CreateResx( $"{root}/locales/en/accounting.resx" );
            var ukAccounting = CreateResx( $"{root}/locales/uk/accounting.resx" );

            var groups = ResourceGroup.Detect( new[] { enCommon, ukCommon, enAccounting, ukAccounting } );

            groups.Should().HaveCount( 2 );
            groups.Should().OnlyContain( g => g.Files.Count == 2 );
            groups.Should().ContainSingle( g =>
                g.Files.All( f => f.FileName == new FileInfo( enCommon ).Name ) );
            groups.Should().ContainSingle( g =>
                g.Files.All( f => f.FileName == new FileInfo( enAccounting ).Name ) );
        }

        [Test]
        public void Detect_NamespaceLayout_SingleLanguage_EachFileOwnGroup()
        {
            // uk-only project: every namespace file is its own group of one (no cross-file grouping).
            var root = UniqueKey();
            var common = CreateResx( $"{root}/locales/uk/common.resx" );
            var accounting = CreateResx( $"{root}/locales/uk/accounting.resx" );

            var groups = ResourceGroup.Detect( new[] { common, accounting } );

            groups.Should().HaveCount( 2 );
            groups.Should().OnlyContain( g => g.Files.Count == 1 && g.BaseFile == null );
        }

        [Test]
        public void Detect_MixedLeftovers_BecomeSingleFileGroups()
        {
            var groupDir = UniqueKey();
            var key = UniqueKey();
            var en = CreateResx( $"{groupDir}/{key}_en.resx" );
            var fr = CreateResx( $"{groupDir}/{key}_fr.resx" );

            // a culture-less file in a different folder cannot join the en/fr group
            var lone = CreateResx( $"{UniqueKey()}/{UniqueKey()}.resx" );

            var groups = ResourceGroup.Detect( new[] { en, fr, lone } );

            groups.Should().HaveCount( 2 );
            groups.Should().ContainSingle( g => g.Files.Count == 2 );
            groups.Should().ContainSingle( g => g.Files.Count == 1 &&
                g.Files[0].FileName == new FileInfo( lone ).Name );
        }

        [Test]
        public void Detect_SingleFile_HasNoBaseFile()
        {
            var file = CreateResx( $"{UniqueKey()}/{UniqueKey()}.resx" );

            var groups = ResourceGroup.Detect( new[] { file } );

            groups.Should().HaveCount( 1 );
            groups[0].Files.Should().HaveCount( 1 );
            groups[0].BaseFile.Should().BeNull();
        }

        [Test]
        public void Detect_BaseFile_PrefersEnglish()
        {
            var dir = UniqueKey();
            var key = UniqueKey();
            var de = CreateResx( $"{dir}/{key}_de.resx" );
            var en = CreateResx( $"{dir}/{key}_en.resx" );
            var fr = CreateResx( $"{dir}/{key}_fr.resx" );

            var groups = ResourceGroup.Detect( new[] { de, en, fr } );

            groups.Should().HaveCount( 1 );
            groups[0].BaseFile.Should().NotBeNull();
            groups[0].BaseFile!.FileName.Should().Be( new FileInfo( en ).Name );
        }

        [Test]
        public void Detect_BaseFile_FallsBackToFirstAlphabetical_WhenNoEnglish()
        {
            var dir = UniqueKey();
            var key = UniqueKey();
            var fr = CreateResx( $"{dir}/{key}_fr.resx" );
            var de = CreateResx( $"{dir}/{key}_de.resx" );

            var groups = ResourceGroup.Detect( new[] { fr, de } );

            groups.Should().HaveCount( 1 );
            groups[0].BaseFile!.FileName.Should().Be( new FileInfo( de ).Name );
        }

        [Test]
        public void Detect_BaseFile_ExplicitBaseLanguage_OverridesAutoPick()
        {
            var dir = UniqueKey();
            var key = UniqueKey();
            var en = CreateResx( $"{dir}/{key}_en.resx" );
            var fr = CreateResx( $"{dir}/{key}_fr.resx" );

            var groups = ResourceGroup.Detect( new[] { en, fr }, baseLanguage: "fr" );

            groups.Should().HaveCount( 1 );
            groups[0].BaseFile!.FileName.Should().Be( new FileInfo( fr ).Name );
        }

        [Test]
        public void Detect_MissingFile_Throws()
        {
            var missing = GetOutputPath( $"{UniqueKey()}.resx" );

            var act = () => ResourceGroup.Detect( new[] { missing } );

            act.Should().Throw<FileNotFoundException>();
        }
    }
}
