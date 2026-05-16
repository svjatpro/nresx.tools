using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles.Xliff
{
    // Reproduces what the random-format CLI validate tests do: load the canonical
    // XLIFF, mangle it with text-level ReplaceKey, then verify the load result.
    [TestFixture]
    public class XliffMangledFileTests : TestBase
    {
        [Test]
        public void Mangled_EmptyKey_LoadsAsEmptyKey()
        {
            var srcPath = GetTestPath( "Resources.xlf" );
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlf );
            File.Copy( srcPath, dstPath, overwrite: true );

            TestHelper.ReplaceKey( dstPath, "Entry2", "" );

            var res = new ResourceFile( dstPath );
            var emptyKeyElement = res.Elements.SingleOrDefault( e => string.IsNullOrEmpty( e.Key ) );

            emptyKeyElement.Should().NotBeNull( "ReplaceKey('Entry2', '') should produce a trans-unit with empty id" );
            emptyKeyElement!.Value.Should().Be( "Value2" );
        }

        [Test]
        public void Mangled_DuplicateKey_LoadsTwoElementsWithSameKey()
        {
            var srcPath = GetTestPath( "Resources.xlf" );
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlf );
            File.Copy( srcPath, dstPath, overwrite: true );

            TestHelper.ReplaceKey( dstPath, "Entry3", "Entry2" );

            var res = new ResourceFile( dstPath );
            var entry2s = res.Elements.Where( e => e.Key == "Entry2" ).ToList();

            entry2s.Should().HaveCount( 2, "after replacement, two trans-units have id='Entry2'" );
        }

        [Test]
        public void Mangled_EmptyValue_LoadsAsEmptyValue()
        {
            var srcPath = GetTestPath( "Resources.xlf" );
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlf );
            File.Copy( srcPath, dstPath, overwrite: true );

            TestHelper.ReplaceKey( dstPath, "Value2", "" );

            var res = new ResourceFile( dstPath );
            var entry2 = res.Elements["Entry2"];

            entry2.Should().NotBeNull();
            entry2!.Value.Should().BeEmpty( "both <source> and <target> for Entry2 were emptied" );
        }

        // Mimics CopyTemporaryFile path: load resx, save as xliff, mangle the result.
        // This is what the CLI random-format tests actually do.
        [Test]
        public void CopyFromResx_ThenMangle_LoadsCorrectly()
        {
            var resxPath = GetTestPath( TestData.ExampleResourceFile );
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xliff );

            // step 1: load resx and save as xliff (mirroring CopyTemporaryFile)
            var src = new ResourceFile( resxPath );
            src.Save( dstPath, ResourceFormatType.Xliff, createDir: true );

            File.Exists( dstPath ).Should().BeTrue();

            // step 2: mangle: replace key "Entry2" with empty
            TestHelper.ReplaceKey( dstPath, "Entry2", "" );

            // step 3: load and verify the empty-key element exists
            var loaded = new ResourceFile( dstPath );
            var emptyKeyElement = loaded.Elements.SingleOrDefault( e => string.IsNullOrEmpty( e.Key ) );

            emptyKeyElement.Should().NotBeNull();
            emptyKeyElement!.Value.Should().Be( "Value2" );
        }
    }
}
