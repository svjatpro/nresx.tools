using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.Core.Tests.Extensions
{
    [TestFixture]
    public class ConvertResourceElementsTests : TestBase
    {
        #region add prefix 

        [Test]
        public async Task AddPrefix()
        {
            var prefix = UniqueKey();
            var res = GetExampleResourceFile();

            res.AddPrefix( prefix );

            res.Elements.All( el => el.Value.StartsWith( prefix ) ).Should().BeTrue();
        }

        [Test]
        public async Task AddPrefixShouldNotDuplicate()
        {
            var prefix = UniqueKey();
            var res = GetExampleResourceFile();

            res.AddPrefix( prefix );
            var elements = res.Elements.Select( el => el.Value ).ToList();
            res.AddPrefix( prefix );

            res.Elements.Select( el => el.Value ).Should().BeEquivalentTo( elements );
        }

        [Test]
        public async Task RemovePrefix()
        {
            var prefix = UniqueKey();
            var res = GetExampleResourceFile();

            var elements = res.Elements.Select( el => el.Value ).ToList();
            res.AddPrefix( prefix );
            res.RemovePrefix( prefix );

            res.Elements.Select( el => el.Value ).Should().BeEquivalentTo( elements );
        }

        [Test]
        public async Task RemovePrefixShouldNotRemoveNotExistingOne()
        {
            var prefix = UniqueKey();
            var res = GetExampleResourceFile();

            var elements = res.Elements.Select( el => el.Value ).ToList();
            res.AddPrefix( prefix );
            res.RemovePrefix( UniqueKey() );
            res.RemovePrefix( prefix );
            res.RemovePrefix( UniqueKey() );

            res.Elements.Select( el => el.Value ).Should().BeEquivalentTo( elements );
        }

        #endregion

        #region add prefix 

        [Test]
        public async Task AddPostfix()
        {
            var postfix = UniqueKey();
            var res = GetExampleResourceFile();

            res.AddPostfix( postfix );

            res.Elements.All( el => el.Value.EndsWith( postfix ) ).Should().BeTrue();
        }

        [Test]
        public async Task AddPostfixShouldNotDuplicate()
        {
            var postfix = UniqueKey();
            var res = GetExampleResourceFile();

            res.AddPostfix( postfix );
            var elements = res.Elements.Select( el => el.Value ).ToList();
            res.AddPostfix( postfix );

            res.Elements.Select( el => el.Value ).Should().BeEquivalentTo( elements );
        }

        [Test]
        public async Task RemovePostfix()
        {
            var postfix = UniqueKey();
            var res = GetExampleResourceFile();

            var elements = res.Elements.Select( el => el.Value ).ToList();
            res.AddPostfix( postfix );
            res.RemovePostfix( postfix );

            res.Elements.Select( el => el.Value ).Should().BeEquivalentTo( elements );
        }

        [Test]
        public async Task RemovePostfixShouldNotRemoveNotExistingOne()
        {
            var postfix = UniqueKey();
            var res = GetExampleResourceFile();

            var elements = res.Elements.Select( el => el.Value ).ToList();
            res.AddPostfix( postfix );
            res.RemovePostfix( UniqueKey() );
            res.RemovePostfix( postfix );
            res.RemovePostfix( UniqueKey() );

            res.Elements.Select( el => el.Value ).Should().BeEquivalentTo( elements );
        }

        #endregion
    }
}