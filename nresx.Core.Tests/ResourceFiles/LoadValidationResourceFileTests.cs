using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.Core.Tests.ResourceFiles
{
    [TestFixture]
    public class LoadValidationResourceFileTests : TestBase
    {
        [Test]
        public void ValidateDuplicatedKeysTest()
        {
            var res = new ResourceFile( GetTestPath( "Resources.txt" ) );
            var targetPath = GetOutputPath( UniqueKey(), res.FileFormat );
            res.Save( targetPath );

            res = new ResourceFile( targetPath );

            res.FileName.Should().Be( Path.GetFileName( targetPath ) );
            res.AbsolutePath.Should().Be( Path.GetFullPath( targetPath ) );
            res.FileFormat.Should().Be( ResourceFormatType.PlainText );

            res.Elements.Select( el => (el.Key, el.Value) ).Should()
                .BeEquivalentTo( GetExampleResourceFile().Elements.Select( el => (el.Value, el.Value) ) );
        }
    }
}