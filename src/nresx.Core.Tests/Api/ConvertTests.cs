using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Exceptions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.Core.Tests.Api
{
    [TestFixture]
    public class ConvertTests : TestBase
    {
        [TestCase( "json" )]
        [TestCase( "po" )]
        [TestCase( "yaml" )]
        public void Convert_DerivesFormatFromDestinationExtension( string extension )
        {
            var source = GetTestPath( TestData.ExampleResourceFile );
            var dest = GetOutputPath( $"{UniqueKey()}.{extension}" );

            ResourceManager.Convert( source, dest );

            new FileInfo( dest ).Exists.Should().BeTrue();

            var expected = new ResourceFile( source ).Elements.Select( e => (e.Key, e.Value) );
            var actual = new ResourceFile( dest ).Elements.Select( e => (e.Key, e.Value) );
            actual.Should().BeEquivalentTo( expected );
        }

        [Test]
        public void Convert_ExplicitFormat_OverridesExtensionlessDestination()
        {
            var source = GetTestPath( TestData.ExampleResourceFile );
            var dest = Path.Combine( TestData.OutputFolder, UniqueKey() ); // no extension

            ResourceManager.Convert( source, dest, ResourceFormatType.Json );

            var written = Path.ChangeExtension( dest, "json" );
            new FileInfo( written ).Exists.Should().BeTrue();
        }

        [Test]
        public void Convert_UnknownDestinationExtension_Throws()
        {
            var source = GetTestPath( TestData.ExampleResourceFile );
            var dest = Path.Combine( TestData.OutputFolder, $"{UniqueKey()}.xyz" );

            var act = () => ResourceManager.Convert( source, dest );

            act.Should().Throw<UnknownResourceFormatException>();
        }
    }
}
