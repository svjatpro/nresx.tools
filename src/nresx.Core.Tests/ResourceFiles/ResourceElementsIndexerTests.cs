using System;
using FluentAssertions;
using nresx.Core;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class ResourceElementsIndexerTests : TestBase
    {
        [Test]
        public void StringIndexer_Set_ExistingKey_Replaces()
        {
            var res = new ResourceFile( ResourceFormatType.Resx );
            res.Elements.Add( "a", "v1" );
            res.Elements.Add( "b", "v2" );

            res.Elements["a"] = new ResourceElement { Key = "a", Value = "replaced" };

            res.Elements["a"]!.Value.Should().Be( "replaced" );
            res.Elements["b"]!.Value.Should().Be( "v2", "untouched element should keep its value" );
        }

        [Test]
        public void StringIndexer_Set_NonExistingKey_Adds()
        {
            // RSX-128 bug fix: old code overwrote element at index 0 when key not found
            var res = new ResourceFile( ResourceFormatType.Resx );
            res.Elements.Add( "first", "v1" );
            res.Elements.Add( "second", "v2" );

            res.Elements["new"] = new ResourceElement { Key = "new", Value = "added" };

            res.Elements["first"]!.Value.Should().Be( "v1", "existing element[0] must not be clobbered" );
            res.Elements["second"]!.Value.Should().Be( "v2" );
            res.Elements["new"]!.Value.Should().Be( "added" );
        }

        [Test]
        public void StringIndexer_Set_Null_ThrowsArgumentNullException()
        {
            var res = new ResourceFile( ResourceFormatType.Resx );
            res.Elements.Add( "a", "v1" );

            Action act = () => res.Elements["a"] = null;

            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void KeyContextIndexer_Set_NonExistingKey_Adds()
        {
            var res = new ResourceFile( ResourceFormatType.Resx );
            res.Elements.Add( "a", "v1", context: "ctx1" );

            res.Elements["a", "ctx2"] = new ResourceElement { Key = "a", Context = "ctx2", Value = "added" };

            res.Elements["a", "ctx1"]!.Value.Should().Be( "v1" );
            res.Elements["a", "ctx2"]!.Value.Should().Be( "added" );
        }
    }
}
