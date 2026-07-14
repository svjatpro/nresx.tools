using System.IO;
using FluentAssertions;
using nresx.CommandLine.Analysis;
using nresx.Core;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Analysis
{
    [TestFixture]
    public class ProjectAnalyzerTests : TestBase
    {
        // Creates an isolated project root under the test output folder and returns its full path.
        private string NewProjectRoot()
        {
            var probe = GetOutputPath( Path.Combine( UniqueKey(), "probe.json" ), ResourceFormatType.NA );
            var root = Path.GetDirectoryName( Path.GetFullPath( probe ) )!;
            Directory.CreateDirectory( root );
            return root;
        }

        private static void Write( string root, string relativePath, string content )
        {
            var full = Path.Combine( root, relativePath.Replace( '/', Path.DirectorySeparatorChar ) );
            Directory.CreateDirectory( Path.GetDirectoryName( full )! );
            File.WriteAllText( full, content );
        }

        [Test]
        public void Analyze_SourceOnly_NotLocalized_CountsTokens()
        {
            var root = NewProjectRoot();
            Write( root, "src/Greeter.cs",
                "namespace A { class G { string Hello() { var m = \"Hello there\"; return m; } string Bye => \"Goodbye\"; } }" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeFalse();
            a.SourceFileCount.Should().Be( 1 );
            a.SourceScanPartial.Should().BeFalse();
            a.PotentialTokens.Should().BeGreaterThan( 0 );
        }

        [Test]
        public void Analyze_NamespaceTree_Localized_DetectsLanguagesAndLayout()
        {
            var root = NewProjectRoot();
            Write( root, "locales/en/common.json", "{ \"title\": \"Home\", \"save\": \"Save\" }" );
            Write( root, "locales/uk/common.json", "{ \"title\": \"Головна\", \"save\": \"Зберегти\" }" );
            Write( root, "locales/en/menu.json", "{ \"file\": \"File\" }" );
            Write( root, "locales/uk/menu.json", "{ \"file\": \"Файл\" }" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeTrue();
            a.ResourceFileCount.Should().Be( 4 );
            a.GroupCount.Should().Be( 2 );
            a.Languages.Should().BeEquivalentTo( "en", "uk" );
            a.Formats.Should().BeEquivalentTo( ".json" );
            a.Layout.Should().Contain( "namespace" );
            // disjoint namespaces are not cross-flagged (RSX-252), values differ -> zero issues
            a.TotalIssues.Should().Be( 0 );
        }

        [Test]
        public void Analyze_PlainJsonWithoutLocaleSignal_TreatedAsNotLocalized()
        {
            var root = NewProjectRoot();
            // a config-style json with no culture and not under a locale folder must not read as localization
            Write( root, "appsettings.json", "{ \"ConnectionString\": \"server=.\" }" );
            Write( root, "src/App.cs", "namespace A { class App { string T => \"Welcome\"; } }" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeFalse();
            a.SourceFileCount.Should().Be( 1 );
        }

        [Test]
        public void Analyze_SatelliteResx_Localized_ReportsSatelliteLayout()
        {
            var root = NewProjectRoot();
            TestHelper.CopyTemporaryFile( destPath: Path.Combine( root, "Strings.resx" ) );
            TestHelper.CopyTemporaryFile( destPath: Path.Combine( root, "Strings.de.resx" ) );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeTrue();
            a.Layout.Should().Contain( "satellite" );
            a.Languages.Should().Contain( "de" );
        }

        [Test]
        public void Analyze_EmptyProject_NotLocalized_NoSource()
        {
            var root = NewProjectRoot();

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeFalse();
            a.SourceFileCount.Should().Be( 0 );
            a.PotentialTokens.Should().Be( 0 );
        }

        // RSX-264: a folder with neither resources nor recognized source is "no project", not
        // a mislabelled "localized" or an empty "not localized" claim.
        [Test]
        public void Analyze_NothingRecognized_ReportsNoProject()
        {
            var root = NewProjectRoot();
            Write( root, "README.md", "# notes" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeFalse();
            a.NoProject.Should().BeTrue();
        }

        // RSX-264: resource files that live only under test/fixture folders do NOT make the
        // project localized - they are reported as a separate fact (the nresx-repo case).
        [Test]
        public void Analyze_ResourcesOnlyInFixtures_NotLocalized_ReportedSeparately()
        {
            var root = NewProjectRoot();
            TestHelper.CopyTemporaryFile( destPath: Path.Combine( root, "test_files", "Strings.resx" ) );
            Write( root, "src/App.cs", "namespace A { class App { string T => \"Welcome\"; } }" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeFalse();
            a.ResourceFileCount.Should().Be( 0 );
            a.FixtureResourceFileCount.Should().Be( 1 );
            a.FixtureFormats.Should().Contain( ".resx" );
            a.SourceFileCount.Should().Be( 1 );
        }

        // RSX-264: a lone config/build file in a weak format is not localization.
        [Test]
        public void Analyze_StrayVersionProperties_NotTreatedAsResource()
        {
            var root = NewProjectRoot();
            Write( root, "version.properties", "version=1.2.3\nbuild=99" );
            Write( root, "src/App.cs", "namespace A { class App { string T => \"Welcome\"; } }" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeFalse();
            a.FixtureResourceFileCount.Should().Be( 0 );
        }

        // RSX-264: a weak-format file DOES count once it forms a real culture group.
        [Test]
        public void Analyze_PropertiesCultureGroup_Localized()
        {
            var root = NewProjectRoot();
            Write( root, "config/messages.properties", "hi=Hello" );
            Write( root, "config/messages_de.properties", "hi=Hallo" );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeTrue();
            a.ResourceFileCount.Should().Be( 2 );
            a.Languages.Should().Contain( "de" );
        }

        // RSX-264: the source scan stops on the budget and reports a partial result so a large
        // repo never hangs the plain run.
        [Test]
        public void Analyze_SourceScan_StopsOnBudget_ReportsPartial()
        {
            var root = NewProjectRoot();
            for ( var i = 0; i < 5; i++ )
                Write( root, $"src/File{i}.cs", $"class C{i} {{ string T => \"hello {i}\"; }}" );

            var a = new ProjectAnalyzer().Analyze( root, new ScanBudget { MaxSourceFiles = 2 } );

            a.Localized.Should().BeFalse();
            a.SourceFileCount.Should().Be( 5 );
            a.SourceFilesScanned.Should().Be( 2 );
            a.SourceScanPartial.Should().BeTrue();
        }

        // RSX-264: the resource scan is likewise bounded and flagged partial.
        [Test]
        public void Analyze_ResourceScan_StopsOnBudget_ReportsPartial()
        {
            var root = NewProjectRoot();
            Write( root, "locales/en/a.json", "{ \"x\": \"1\" }" );
            Write( root, "locales/uk/a.json", "{ \"x\": \"2\" }" );
            Write( root, "locales/en/b.json", "{ \"y\": \"3\" }" );
            Write( root, "locales/uk/b.json", "{ \"y\": \"4\" }" );

            var a = new ProjectAnalyzer().Analyze( root, new ScanBudget { MaxResourceFiles = 2 } );

            a.Localized.Should().BeTrue();
            a.ResourceFileCount.Should().Be( 4 );
            a.ResourceFilesScanned.Should().Be( 2 );
            a.ResourceScanPartial.Should().BeTrue();
        }

        // RSX-264: an unbounded budget scans everything and reports a complete result.
        [Test]
        public void Analyze_UnlimitedBudget_ScansAll_NotPartial()
        {
            var root = NewProjectRoot();
            for ( var i = 0; i < 5; i++ )
                Write( root, $"src/File{i}.cs", $"class C{i} {{ string T => \"hello {i}\"; }}" );

            var a = new ProjectAnalyzer().Analyze( root, ScanBudget.Unlimited );

            a.SourceFilesScanned.Should().Be( 5 );
            a.SourceScanPartial.Should().BeFalse();
        }

        // RSX-264: a small localized project lists the resource files, not just a count.
        [Test]
        public void Analyze_FewResourceFiles_ListsTheirPaths()
        {
            var root = NewProjectRoot();
            TestHelper.CopyTemporaryFile( destPath: Path.Combine( root, "Strings.resx" ) );

            var a = new ProjectAnalyzer().Analyze( root );

            a.Localized.Should().BeTrue();
            a.ResourceFilePaths.Should().NotBeEmpty();
            a.ResourceFilePaths.Should().Contain( p => p.Contains( "Strings.resx" ) );
        }
    }
}
