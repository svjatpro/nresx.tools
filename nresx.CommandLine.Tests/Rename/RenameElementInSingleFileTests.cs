using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Rename
{
    [TestFixture]
    public class RenameElementInSingleFileTests : TestBase
    {
        [TestCase( @"rename [TmpFile] -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [TmpFile] --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters{UniqueKeys = { elementToUpdate.Key }} );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            var element = res.Elements.First( el => el.Key == args.UniqueKeys[0] );
            element.Should().BeEquivalentTo( elementToUpdate, config => config.Excluding( el => el.Key ) );
        }

        [TestCase( @"rename [TmpFile] -k [UniqueKey] -n [UniqueKey] --dry-run" )]
        [TestCase( @"rename [TmpFile] --key [UniqueKey] --new-key [UniqueKey] --dry-run" )]
        public void RenameSingleElementDryRun( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementToUpdate.Key } } );

            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.FirstOrDefault( el => el.Key == args.UniqueKeys[0] ).Should().BeNull();

            args.ConsoleOutput.Should().BeEquivalentTo( $"'{elementToUpdate.Key}' element have been renamed to '{args.UniqueKeys[0]}' in '{args.TemporaryFiles[0]}'" );
        }



        [TestCase( @"rename [Output]\[UniqueKey].resx -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [Output]\[UniqueKey].resx --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameInNonExistingFile( string commandLine )
        {
            var args = Run( commandLine );

            var file = GetOutputPath( args.UniqueKeys[0] );
            new FileInfo( file ).Exists.Should().BeFalse();

            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( FilesNotFoundErrorMessage, file ) );
        }

        [TestCase( @"rename [UniqueKey]\[UniqueKey].resx -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [UniqueKey]\[UniqueKey].resx --key [UniqueKey] --new-key [UniqueKey]" )]
        [TestCase( @"rename [UniqueKey]\[UniqueKey].resx --key [UniqueKey] --new-key [UniqueKey] --new-file --recursive" )] // ignored for rename
        public void RenameNonExistingDir( string commandLine )
        {
            var args = Run( commandLine );

            var file = $"{args.UniqueKeys[0]}\\{args.UniqueKeys[1]}.resx";
            new FileInfo( file ).Exists.Should().BeFalse();

            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( DirectoryNotFoundErrorMessage, file ) );
        }

        [TestCase( @"rename [TmpFile] -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [TmpFile] --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameNonExistingElement( string commandLine )
        {
            var args = Run( commandLine );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            var element = res.Elements.FirstOrDefault( el => el.Key == args.UniqueKeys[0] );
            element?.Should().BeNull();

            args.ConsoleOutput.Should().BeEquivalentTo( $"fatal: '{args.UniqueKeys[0]}' element not found" );
        }
    }
}