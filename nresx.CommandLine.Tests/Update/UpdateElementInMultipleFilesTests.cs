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
        [TestCase( @"update [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update -s [TmpFile] [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [TmpFile] [TmpFile] --key [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        [TestCase( @"update -s [TmpFile] [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { elementToUpdate.Key } } )
                .WithParams( args => new
                {
                    key = elementToUpdate.Key,
                    value = args.UniqueKeys[1],
                    comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : null
                } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( ( args, param ) =>
                {
                    args.TemporaryFiles.ForEach(
                        file =>
                        {
                            var res = new ResourceFile( file );
                            res.Elements.Should().NotContain( el =>
                                el.Key == param.key &&
                                el.Value == param.value &&
                                ( ( res.ElementHasComment && param.comment != null && el.Comment == param.comment ) || true ) );
                        } );
                } )
                .ValidateRun( ( args, param ) =>
                {
                    args.TemporaryFiles.ForEach(
                        file =>
                        {
                            var res = new ResourceFile( file );
                            res.Elements.Should().Contain( el => 
                                el.Key == param.key && 
                                el.Value == param.value &&
                                ( ( res.ElementHasComment && param.comment != null && el.Comment == param.comment ) || true ) );
                        } );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.key}' element have been updated in '{args.TemporaryFiles[0]}'",
                        $"'{param.key}' element have been updated in '{args.TemporaryFiles[1]}'" );
                } );
        }
        
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateSingleElementByNonRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } )
                .WithParams( args => new
                {
                    updated = new ResourceElement
                    {
                        Key = elementToUpdate.Key,
                        Value = args.UniqueKeys[2],
                        Comment = args.UniqueKeys.Count > 3 ? args.UniqueKeys[3] : elementToUpdate.Comment,
                        Type = elementToUpdate.Type
                    }
                } )
                .ValidateDryRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                    new ResourceFile( files[1] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                    new ResourceFile( files[2] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                } )
                .ValidateRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( param.updated );
                    new ResourceFile( files[1] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( param.updated );
                    new ResourceFile( files[2] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.updated.Key}' element have been updated in '{files[0].GetShortPath()}'",
                        $"'{param.updated.Key}' element have been updated in '{files[1].GetShortPath()}'" );
                } );
        }
        
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -r" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] -r" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --recursive" )]
        [TestCase( @"update [Output]\[UniqueKey]*.resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --recursive" )]
        public void UpdateSingleElementByRecursiveSpec( string commandLine )
        {
            var elementToUpdate = GetExampleResourceFile().Elements[1];
            var files = PrepareTemporaryFiles( 2, 1, out var fileKey );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { fileKey, elementToUpdate.Key } } )
                .WithParams( args => new
                {
                    updated = new ResourceElement
                    {
                        Key = elementToUpdate.Key,
                        Value = args.UniqueKeys[2],
                        Comment = args.UniqueKeys.Count > 3 ? args.UniqueKeys[3] : elementToUpdate.Comment,
                        Type = elementToUpdate.Type
                    }
                } )
                .ValidateDryRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                    new ResourceFile( files[1] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                    new ResourceFile( files[2] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( elementToUpdate );
                } )
                .ValidateRun( ( _, param ) =>
                {
                    new ResourceFile( files[0] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( param.updated );
                    new ResourceFile( files[1] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( param.updated );
                    new ResourceFile( files[2] ).Elements.Single( el => el.Key == param.updated.Key ).Should().BeEquivalentTo( param.updated );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.updated.Key}' element have been updated in '{files[0].GetShortPath()}'",
                        $"'{param.updated.Key}' element have been updated in '{files[1].GetShortPath()}'",
                        $"'{param.updated.Key}' element have been updated in '{files[2].GetShortPath()}'" );
                } );
        }
    }
}