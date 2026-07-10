using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.Core.Tests.Api
{
    [TestFixture]
    public class DiffTests : TestBase
    {
        private string CopyExample( string extension = "resx" )
        {
            var path = GetOutputPath( $"{UniqueKey()}.{extension}" );
            TestHelper.CopyTemporaryFile( destPath: path );
            return path;
        }

        [Test]
        public void Diff_IdenticalFiles_AreSame()
        {
            var first = CopyExample();
            var second = CopyExample();

            var diff = ResourceManager.Diff( first, second );

            diff.AreSame.Should().BeTrue();
            diff.AddedElements.Should().BeEmpty();
            diff.RemovedElements.Should().BeEmpty();
            diff.ChangedElements.Should().BeEmpty();
        }

        [Test]
        public void Diff_AddedKey_ListedInAddedElements()
        {
            var first = CopyExample();
            var second = CopyExample();

            var res = new ResourceFile( second );
            var addedKey = UniqueKey();
            res.Elements.Add( addedKey, UniqueKey() );
            res.Save( second );

            var diff = ResourceManager.Diff( first, second );

            diff.AreSame.Should().BeFalse();
            diff.AddedElements.Select( e => e.Key ).Should().ContainSingle().Which.Should().Be( addedKey );
            diff.RemovedElements.Should().BeEmpty();
            diff.ChangedElements.Should().BeEmpty();
        }

        [Test]
        public void Diff_RemovedKey_ListedInRemovedElements()
        {
            var first = CopyExample();
            var second = CopyExample();

            var res = new ResourceFile( second );
            var removedKey = res.Elements[0]!.Key;
            res.Elements.Remove( removedKey );
            res.Save( second );

            var diff = ResourceManager.Diff( first, second );

            diff.AreSame.Should().BeFalse();
            diff.RemovedElements.Select( e => e.Key ).Should().ContainSingle().Which.Should().Be( removedKey );
            diff.AddedElements.Should().BeEmpty();
            diff.ChangedElements.Should().BeEmpty();
        }

        [Test]
        public void Diff_ChangedValue_ListedInChangedElements()
        {
            var first = CopyExample();
            var second = CopyExample();

            var res = new ResourceFile( second );
            var changedKey = res.Elements[1]!.Key;
            var newValue = UniqueKey();
            res.Elements[1]!.Value = newValue;
            res.Save( second );

            var diff = ResourceManager.Diff( first, second );

            diff.AreSame.Should().BeFalse();
            diff.AddedElements.Should().BeEmpty();
            diff.RemovedElements.Should().BeEmpty();
            diff.ChangedElements.Should().ContainSingle( c => c.Second.Key == changedKey && c.Second.Value == newValue );
        }

        [Test]
        public void Diff_CrossFormat_SameContent_AreSame()
        {
            var resx = CopyExample( "resx" );
            var po = GetOutputPath( $"{UniqueKey()}.po" );
            ResourceManager.Convert( resx, po );

            var diff = ResourceManager.Diff( resx, po );

            diff.AreSame.Should().BeTrue();
        }
    }
}
