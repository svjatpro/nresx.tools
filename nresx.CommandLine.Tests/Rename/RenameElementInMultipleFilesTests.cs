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
        [TestCase( @"rename [TmpFile] [TmpFile] -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [TmpFile] [TmpFile] --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { elementToUpdate.Key } } )
                .WithParams( args => new { oldKey = elementToUpdate.Key, newKey = args.UniqueKeys[1] } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( ( args, param ) =>
                {
                    args.TemporaryFiles.ForEach(
                        file =>
                        {
                            var res = new ResourceFile( file );
                            res.Elements.FirstOrDefault( el => el.Key == param.newKey ).Should().BeNull();
                        } );
                } )
                .ValidateRun( ( args, param ) =>
                {
                    args.TemporaryFiles.ForEach(
                        file =>
                        {
                            var res = new ResourceFile( file );
                            var element = res.Elements.First( el => el.Key == param.newKey );
                            element.Should().BeEquivalentTo( elementToUpdate, config => config.Excluding( el => el.Key ).Excluding( el => el.Comment ) );
                        } );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{args.TemporaryFiles[0]}'",
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{args.TemporaryFiles[1]}'" );
                } );
        }

        [TestCase( @"rename [Output]\[UniqueKey]* -k [UniqueKey] -n [UniqueKey]" )]
        [TestCase( @"rename [Output]\[UniqueKey]* --key [UniqueKey] --new-key [UniqueKey]" )]
        public void RenameSingleElementByNonRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } )
                .WithParams( args => new { oldKey = elementToUpdate.Key, newKey = args.UniqueKeys[2] } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                    new ResourceFile( files[1] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                    new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                } )
                .ValidateRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Should().ContainSingle( el => el.Key == param.newKey );
                    new ResourceFile( files[1] ).Elements.Should().ContainSingle( el => el.Key == param.newKey );
                    new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{files[0].GetShortPath()}'",
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{files[1].GetShortPath()}'" );
                } );
        }

        [TestCase( @"rename [Output]\[UniqueKey]* -k [UniqueKey] -n [UniqueKey] -r" )]
        [TestCase( @"rename [Output]\[UniqueKey]* --key [UniqueKey] --new-key [UniqueKey] --recursive" )]
        public void RenameSingleElementByRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } )
                .WithParams( args => new { oldKey = elementToUpdate.Key, newKey = args.UniqueKeys[2] } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                    new ResourceFile( files[1] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                    new ResourceFile( files[2] ).Elements.Should().NotContain( el => el.Key == param.newKey );
                } )
                .ValidateRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Should().ContainSingle( el => el.Key == param.newKey );
                    new ResourceFile( files[1] ).Elements.Should().ContainSingle( el => el.Key == param.newKey );
                    new ResourceFile( files[2] ).Elements.Should().ContainSingle( el => el.Key == param.newKey );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{files[0].GetShortPath()}'",
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{files[1].GetShortPath()}'",
                        $"'{param.oldKey}' element have been renamed to '{param.newKey}' in '{files[2].GetShortPath()}'" );
                } );
        }
    }
}