using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Add
{
    [TestFixture]
    [Parallelizable( ParallelScope.Self )]
    public class AddElementMultipleResourcesTests : TestBase
    {
        [TestCase( @"add [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"add [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"add [TmpFile] [TmpFile] --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add [TmpFile] [TmpFile] --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void AddSingleElementToListOfResources( string commandLine )
        {
            var args = Run( commandLine );

            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];
            var comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : string.Empty;

            args.TemporaryFiles.ForEach(
                file =>
                {
                    var res = new ResourceFile( file );
                    res.Elements.Should().Contain( el => el.Key == key && el.Value == value && el.Comment == comment );
                } );
        }
        [TestCase( @"add [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] --dry-run" )]
        [TestCase( @"add [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --dry-run" )]
        [TestCase( @"add [TmpFile] [TmpFile] --key [UniqueKey] --value [UniqueKey] --dry-run" )]
        [TestCase( @"add [TmpFile] [TmpFile] --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --dry-run" )]
        public void AddSingleElementToListOfResourcesDryRun( string commandLine )
        {
            var args = Run( commandLine );

            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"'{key}: {value}' element have been add to '{args.TemporaryFiles[0]}'",
                $"'{key}: {value}' element have been add to '{args.TemporaryFiles[1]}'" );
        }



        [TestCase( @"add [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"add [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"add [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void AddSingleElementToByNonRecursiveSpec( string commandLine )
        {
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters{UniqueKeys = { fileKey }} );

            var key = args.UniqueKeys[0]; 
            var value = args.UniqueKeys[1];
            var comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : string.Empty;

            new ResourceFile( files[0] ).Elements.Should().Contain( el => el.Key == key && el.Value == value && el.Comment == comment );
            new ResourceFile( files[1] ).Elements.Should().Contain( el => el.Key == key && el.Value == value && el.Comment == comment );
            new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == key && el.Value == value && el.Comment == comment );
        }

        [TestCase( @"add [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] --dry-run" )]
        [TestCase( @"add [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --dry-run" )]
        [TestCase( @"add [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --dry-run" )]
        [TestCase( @"add [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --dry-run" )]
        public void AddSingleElementToByNonRecursiveSpecDryRun( string commandLine )
        {
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey } } );

            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];
            
            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{key}: {value}' element have been add to '{files[0]}'",
                $"'{key}: {value}' element have been add to '{files[1]}'" );
        }



        [TestCase( @"add [Output]\[UniqueKey]*.* -k [UniqueKey] -v [UniqueKey] -r" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* -r -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] -r" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* --key [UniqueKey] --value [UniqueKey] --recursive" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* --recursive --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --recursive" )]
        public void AddSingleElementToByRecursiveSpec( string commandLine )
        {
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey } } );

            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];
            var comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : string.Empty;

            new ResourceFile( files[0] ).Elements.Should().Contain( el => el.Key == key && el.Value == value && el.Comment == comment );
            new ResourceFile( files[1] ).Elements.Should().Contain( el => el.Key == key && el.Value == value && el.Comment == comment );
            new ResourceFile( files[2] ).Elements.Should().Contain( el => el.Key == key && el.Value == value && el.Comment == comment );
        }

        [TestCase( @"add [Output]\[UniqueKey]*.* -k [UniqueKey] -v [UniqueKey] -r --dry-run" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] -r --dry-run" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* --key [UniqueKey] --value [UniqueKey] --recursive --dry-run" )]
        [TestCase( @"add [Output]\[UniqueKey]*.* --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --recursive --dry-run" )]
        public void AddSingleElementToByRecursiveSpecDryRun( string commandLine )
        {
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { fileKey } } );

            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{key}: {value}' element have been add to '{files[0].GetShortPath()}'",
                $"'{key}: {value}' element have been add to '{files[1].GetShortPath()}'",
                $"'{key}: {value}' element have been add to '{files[2].GetShortPath()}'" );
        }
    }
}