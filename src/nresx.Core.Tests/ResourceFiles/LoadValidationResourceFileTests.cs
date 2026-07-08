using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core;
using nresx.Core.Exceptions;
using nresx.Core.Extensions;
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

        // Builds a corrupt XLIFF on disk (duplicate key Entry2) - the canonical "broken file" fixture
        // for the three load-mode tests below.
        private string BuildCorruptXliff()
        {
            var srcPath = GetTestPath( "Resources.xlf" );
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlf );
            File.Copy( srcPath, dstPath, overwrite: true );
            TestHelper.ReplaceKey( dstPath, "Entry3", "Entry2" );
            return dstPath;
        }

        [Test]
        public void Strict_DuplicateKey_Throws()
        {
            var path = BuildCorruptXliff();

            Action act = () => new ResourceFile( path );

            var ex = act.Should().Throw<ValidationException>().Which;
            ex.Errors.Should().Contain( e => e.ErrorType == ResourceElementErrorType.Duplicate );
        }

        [Test]
        public void Lenient_DuplicateKey_CollectsAndLoads()
        {
            var path = BuildCorruptXliff();

            var res = new ResourceFile( path, new ResourceFileOption { LoadMode = LoadMode.Lenient } );

            res.Elements.Where( e => e.Key == "Entry2" ).Should().HaveCount( 2 );
            res.ValidationErrors.Should().Contain( e => e.ErrorType == ResourceElementErrorType.Duplicate );
        }

        [Test]
        public void Raw_DuplicateKey_LoadsWithNoValidation()
        {
            var path = BuildCorruptXliff();

            var res = new ResourceFile( path, new ResourceFileOption { LoadMode = LoadMode.Raw } );

            res.Elements.Where( e => e.Key == "Entry2" ).Should().HaveCount( 2 );
            res.ValidationErrors.Should().BeEmpty( "Raw mode skips validation entirely" );
        }

        [Test]
        public void Validate_CanBeCalledManually_AfterRawLoad()
        {
            var path = BuildCorruptXliff();
            var res = new ResourceFile( path, new ResourceFileOption { LoadMode = LoadMode.Raw } );

            res.ValidationErrors.Should().BeEmpty();

            var errors = res.Validate();

            errors.Should().Contain( e => e.ErrorType == ResourceElementErrorType.Duplicate );
            res.ValidationErrors.Should().BeEquivalentTo( errors, "Validate() refreshes the property" );
        }

        [Test]
        public void Strict_EmptyValue_DoesNotThrow_ItIsWarningOnly()
        {
            // EmptyValue is severity=Warning per RSX-229, so Strict does NOT throw
            var srcPath = GetTestPath( "Resources.xlf" );
            var dstPath = GetOutputPath( UniqueKey(), ResourceFormatType.Xlf );
            File.Copy( srcPath, dstPath, overwrite: true );
            TestHelper.ReplaceKey( dstPath, "Value2", "" );

            Action act = () => new ResourceFile( dstPath );

            act.Should().NotThrow();
        }
    }
}