using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class AsyncResourceFileTests : TestBase
    {
        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task LoadAsyncFromPath_MatchesSyncCtor( string path )
        {
            var sync = new ResourceFile( GetTestPath( path ) );
            var async = await ResourceFile.LoadAsync( GetTestPath( path ) );

            async.FileFormat.Should().Be( sync.FileFormat );
            async.FileName.Should().Be( sync.FileName );
            async.AbsolutePath.Should().Be( sync.AbsolutePath );
            async.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( sync.Elements.Select( el => (el.Key, el.Value) ) );
        }

        [Test]
        public async Task LoadAsyncFromPath_PreservesFileNameAndPath()
        {
            var path = GetTestPath( TestData.ExampleResourceFile );
            var res = await ResourceFile.LoadAsync( path );

            res.FileName.Should().Be( Path.GetFileName( path ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( path ) );
            res.IsNewFile.Should().BeFalse();
        }

        [Test]
        public async Task LoadAsyncFromPath_NonExistingFile_IsNewFile()
        {
            var path = GetOutputPath( UniqueKey(), ResourceFormatType.Resx );
            var res = await ResourceFile.LoadAsync( path );

            res.IsNewFile.Should().BeTrue();
            res.Elements.Should().BeEmpty();
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task LoadAsyncFromStream_MatchesSyncCtor( string path )
        {
            var fullPath = GetTestPath( path );

            var sync = new ResourceFile( fullPath );

            using ( var fs = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) )
            {
                var async = await ResourceFile.LoadAsync( fs );

                async.FileFormat.Should().Be( sync.FileFormat );
                async.Elements.Select( el => (el.Key, el.Value) ).Should()
                    .BeEquivalentTo( sync.Elements.Select( el => (el.Key, el.Value) ) );
            }
        }

        [Test]
        public async Task LoadAsyncFromFileStream_PreservesPath()
        {
            var path = GetTestPath( TestData.ExampleResourceFile );

            await using var fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var res = await ResourceFile.LoadAsync( fs );

            res.FileName.Should().Be( Path.GetFileName( path ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( path ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task SaveAsyncToPath_RoundTrip( string path )
        {
            var source = new ResourceFile( GetTestPath( path ) );
            var targetPath = GetOutputPath( UniqueKey(), source.FileFormat );

            await source.SaveAsync( targetPath );

            File.Exists( targetPath ).Should().BeTrue();

            var loaded = await ResourceFile.LoadAsync( targetPath );
            loaded.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value) ) );
        }

        [TestCaseSource( typeof( TestData ), nameof( TestData.ResourceFiles ) )]
        public async Task SaveAsyncToStream_RoundTrip( string path )
        {
            var source = new ResourceFile( GetTestPath( path ) );

            using var ms = new MemoryStream();
            await source.SaveAsync( ms, source.FileFormat );
            ms.Position = 0;

            var loaded = await ResourceFile.LoadAsync( ms, source.FileFormat );
            loaded.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( source.Elements.Select( el => (el.Key, el.Value) ) );
        }

        [Test]
        public async Task LoadRawElementsAsync_FromPath_ReturnsElements()
        {
            var path = GetTestPath( TestData.ExampleResourceFile );

            var elements = await ResourceFile.LoadRawElementsAsync( path );

            elements.Should().NotBeEmpty();
        }

        [Test]
        public async Task LoadRawElementsAsync_FromStream_ReturnsElements()
        {
            var path = GetTestPath( TestData.ExampleResourceFile );

            await using var fs = new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
            var elements = await ResourceFile.LoadRawElementsAsync( fs );

            elements.Should().NotBeEmpty();
        }

        [Test]
        public void LoadAsync_CancellationToken_Cancels()
        {
            var path = GetTestPath( TestData.ExampleResourceFile );
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<System.OperationCanceledException>(
                async () => await ResourceFile.LoadAsync( path, cancellationToken: cts.Token ) );
        }

        [Test]
        public void SaveAsync_CancellationToken_Cancels()
        {
            var source = new ResourceFile( GetTestPath( TestData.ExampleResourceFile ) );
            var targetPath = GetOutputPath( UniqueKey(), source.FileFormat );

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.ThrowsAsync<System.OperationCanceledException>(
                async () => await source.SaveAsync( targetPath, cancellationToken: cts.Token ) );
        }
    }
}
