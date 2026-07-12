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
            a.SourceTooLarge.Should().BeFalse();
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
    }
}
