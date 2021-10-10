using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Rename
{
    [TestFixture]
    public class RenameElementInMultipleFilesTests : TestBase
    {
        [TestCase( @"rename [TmpFile.resx] [TmpFile.po] -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [TmpFile.resx] [TmpFile.po] --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { elementToUpdate.Key }} );

            args.TemporaryFiles.ForEach(
                file =>
                {
                    var res = new ResourceFile( file );
                    var element = res.Elements.First( el => el.Key == args.UniqueKeys[0] );
                    element.Should().BeEquivalentTo( elementToUpdate, config => config.Excluding( el => el.Key ) );
                } );
        }

        [TestCase( @"rename [TmpFile] [TmpFile] -k [UniqueKey] -n [UniqueKey] --dry-run" )]
        [TestCase( @"rename [TmpFile] [TmpFile] --key [UniqueKey] --new-key [UniqueKey] --dry-run" )]
        public void RenameSingleElementDryRun( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementToUpdate.Key } } );

            args.TemporaryFiles.ForEach(
                file =>
                {
                    var res = new ResourceFile( file );
                    res.Elements.FirstOrDefault( el => el.Key == args.UniqueKeys[0] ).Should().BeNull();
                } );

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"'{elementToUpdate.Key}' element have been renamed to '{args.UniqueKeys[0]}' in '{args.TemporaryFiles[0]}'",
                $"'{elementToUpdate.Key}' element have been renamed to '{args.UniqueKeys[0]}' in '{args.TemporaryFiles[1]}'" );
        }



        [TestCase( @"rename [Output]\[UniqueKey]*.resx -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [Output]\[UniqueKey]*.resx --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameSingleElementByNonRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = args.UniqueKeys[0];

            new ResourceFile( files[0] ).Elements.Should().ContainSingle( el => el.Key == key );
            new ResourceFile( files[1] ).Elements.Should().ContainSingle( el => el.Key == key );
            new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == key );
        }

        [TestCase( @"rename [Output]\[UniqueKey]*.resx -k [UniqueKey] -n [UniqueKey] --dry-run" )]
        [TestCase( @"rename [Output]\[UniqueKey]*.resx --key [UniqueKey] --new-key [UniqueKey] --dry-run" )]
        public void RenameSingleElementByNonRecursiveSpecDryRun( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = args.UniqueKeys[0];

            new ResourceFile( files[0] ).Elements.Should().NotContain( el => el.Key == key );
            new ResourceFile( files[1] ).Elements.Should().NotContain( el => el.Key == key );
            new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == key );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{elementToUpdate.Key}' element have been renamed to '{key}' in '{files[0]}'",
                $"'{elementToUpdate.Key}' element have been renamed to '{key}' in '{files[1]}'" );
        }




        [TestCase( @"rename [Output]\[UniqueKey]*.resx -k [UniqueKey] -n [UniqueKey] -r" )]
        [TestCase( @"rename [Output]\[UniqueKey]*.resx --key [UniqueKey] --new-key [UniqueKey] --recursive" )]
        public void RenameSingleElementByRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = args.UniqueKeys[0];

            new ResourceFile( files[0] ).Elements.Should().ContainSingle( el => el.Key == key );
            new ResourceFile( files[1] ).Elements.Should().ContainSingle( el => el.Key == key );
            new ResourceFile( files[2] ).Elements.Should().ContainSingle( el => el.Key == key );
        }

        [TestCase( @"rename [Output]\[UniqueKey]*.resx -k [UniqueKey] -n [UniqueKey] -r --dry-run" )]
        [TestCase( @"rename [Output]\[UniqueKey]*.resx --key [UniqueKey] --new-key [UniqueKey] --recursive --dry-run" )]
        public void RenameSingleElementByRecursiveSpecDryRun( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = args.UniqueKeys[0];

            new ResourceFile( files[0] ).Elements.Should().NotContain( el => el.Key == key );
            new ResourceFile( files[1] ).Elements.Should().NotContain( el => el.Key == key );
            new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == key );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{elementToUpdate.Key}' element have been renamed to '{key}' in '{files[0].GetShortPath()}'",
                $"'{elementToUpdate.Key}' element have been renamed to '{key}' in '{files[1].GetShortPath()}'",
                $"'{elementToUpdate.Key}' element have been renamed to '{key}' in '{files[2].GetShortPath()}'" );
        }
    }
}