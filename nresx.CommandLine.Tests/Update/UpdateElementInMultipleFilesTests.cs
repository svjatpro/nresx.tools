using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Update
{
    [TestFixture]
    public class UpdateElementInMultipleFilesTests : TestBase
    {
        [TestCase( @"update [TmpFile.resx] [TmpFile.resx] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile.resx] [TmpFile.po] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update -s [TmpFile.resw] [TmpFile.po] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [TmpFile.po] [TmpFile.po] --key [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile.resx] [TmpFile.resw] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        [TestCase( @"update -s [TmpFile.po] [TmpFile.resx] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { elementToUpdate.Key }} );

            var value = args.UniqueKeys[0];
            var comment = args.UniqueKeys.Count > 1 ? args.UniqueKeys[1] : elementToUpdate.Comment;
            args.TemporaryFiles.ForEach(
                file =>
                {
                    var res = new ResourceFile( file );
                    res.Elements.Should().Contain( el => el.Key == elementToUpdate.Key && el.Value == value && el.Comment == comment );
                } );
        }

        [TestCase( @"update [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] --dry-run" )]
        [TestCase( @"update [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --dry-run" )]
        [TestCase( @"update -s [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --dry-run" )]
        [TestCase( @"update [TmpFile] [TmpFile] --key [UniqueKey] -v [UniqueKey] --dry-run" )]
        [TestCase( @"update [TmpFile] [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey] --dry-run" )]
        [TestCase( @"update -s [TmpFile] [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey] --dry-run" )]
        public void UpdateSingleElementDryRun( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementToUpdate.Key } } );

            var value = args.UniqueKeys[0];
            var comment = args.UniqueKeys.Count > 1 ? args.UniqueKeys[1] : elementToUpdate.Comment;
            args.TemporaryFiles.ForEach(
                file =>
                {
                    var res = new ResourceFile( file );
                    res.Elements.Should().NotContain( el => el.Key == elementToUpdate.Key && el.Value == value && el.Comment == comment );
                } );

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"'{elementToUpdate.Key}' element have been updated in '{args.TemporaryFiles[0]}'",
                $"'{elementToUpdate.Key}' element have been updated in '{args.TemporaryFiles[1]}'" );
        }


        
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateSingleElementByNonRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = elementToUpdate.Key;
            var updated = new ResourceElement
            {
                Key = key,
                Value = args.UniqueKeys[0],
                Comment = args.UniqueKeys.Count > 1 ? args.UniqueKeys[1] : elementToUpdate.Comment,
                Type = elementToUpdate.Type,
            };

            new ResourceFile( files[0] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( updated );
            new ResourceFile( files[1] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( updated );
            new ResourceFile( files[2] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );
        }

        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] --dry-run" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --dry-run" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --dry-run" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --dry-run" )]
        public void UpdateSingleElementByNonRecursiveSpecDryRun( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = elementToUpdate.Key;

            new ResourceFile( files[0] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );
            new ResourceFile( files[1] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );
            new ResourceFile( files[2] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{elementToUpdate.Key}' element have been updated in '{files[0]}'",
                $"'{elementToUpdate.Key}' element have been updated in '{files[1]}'" );
        }




        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -r" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] -r" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --recursive" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --recursive" )]
        public void UpdateSingleElementByRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = elementToUpdate.Key;
            var updated = new ResourceElement
            {
                Key = key,
                Value = args.UniqueKeys[0],
                Comment = args.UniqueKeys.Count > 1 ? args.UniqueKeys[1] : elementToUpdate.Comment,
                Type = elementToUpdate.Type,
            };

            new ResourceFile( files[0] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( updated );
            new ResourceFile( files[1] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( updated );
            new ResourceFile( files[2] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( updated );
        }

        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -r --dry-run" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] -r --dry-run" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --recursive --dry-run" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --recursive --dry-run" )]
        public void UpdateSingleElementByRecursiveSpecDryRun( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } );

            var key = elementToUpdate.Key;

            new ResourceFile( files[0] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );
            new ResourceFile( files[1] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );
            new ResourceFile( files[2] ).Elements.Single( el => el.Key == key ).Should().BeEquivalentTo( elementToUpdate );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{elementToUpdate.Key}' element have been updated in '{files[0].GetShortPath()}'",
                $"'{elementToUpdate.Key}' element have been updated in '{files[1].GetShortPath()}'",
                $"'{elementToUpdate.Key}' element have been updated in '{files[2].GetShortPath()}'" );
        }
    }
}