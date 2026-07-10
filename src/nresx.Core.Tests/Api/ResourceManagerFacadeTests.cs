using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Extensions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.Core.Tests.Api
{
    [TestFixture]
    public class ResourceManagerFacadeTests : TestBase
    {
        private List<string> CreateGroup( string[] locales )
        {
            var dir = UniqueKey();
            var key = UniqueKey();
            var paths = new List<string>();
            foreach ( var locale in locales )
            {
                var path = GetOutputPath( Path.Combine( dir, $"{key}_{locale}.resx" ) );
                TestHelper.CopyTemporaryFile( destPath: path );
                paths.Add( path );
            }
            return paths;
        }

        [Test]
        public void LoadGroups_ReturnsDetectedGroupWithBaseFile()
        {
            var files = CreateGroup( new[] { "en", "fr" } );

            var groups = ResourceManager.LoadGroups( files );

            groups.Should().HaveCount( 1 );
            groups[0].Files.Should().HaveCount( 2 );
            groups[0].BaseFile!.FileName.Should().Be( new FileInfo( files[0] ).Name );
        }

        [Test]
        public void Validate_FlagsNotTranslatedAcrossGroup()
        {
            var files = CreateGroup( new[] { "en", "fr" } );

            // en is the base; fr[1] keeps the en value (untranslated), the rest differ
            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0]!.Value = UniqueKey();
            resFr.Elements[2]!.Value = UniqueKey();
            var untranslatedKey = resFr.Elements[1]!.Key;
            resFr.Save( files[1] );

            var issues = ResourceManager.Validate( files );

            issues.Should().ContainSingle( i =>
                i.Error.ErrorType == ResourceElementErrorType.NotTranslated &&
                i.Error.ElementKey == untranslatedKey );
        }
    }
}
