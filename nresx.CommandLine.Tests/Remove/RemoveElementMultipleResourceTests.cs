using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Remove
{
    [TestFixture]
    public class RemoveElementMultipleResourceTests : TestBase
    {
        #region Remove element(s) by key(s)

        [TestCase( @"remove [TmpFile] [TmpFile] -k [UniqueKey]" )]
        [TestCase( @"remove [TmpFile] [TmpFile] --key [UniqueKey]" )]
        [TestCase( @"remove -s [TmpFile] [TmpFile] -k [UniqueKey]" )]
        [TestCase( @"remove --source [TmpFile] [TmpFile] --key [UniqueKey]" )]
        public void RemoveSingleElementByKey( string commandLine )
        {
            var resTemplate = GetExampleResourceFile();
            var elementToDelete = resTemplate.Elements[1].Key;

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { elementToDelete } } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( args =>
                {
                    new ResourceFile( args.TemporaryFiles[0] ).Elements.Should().Contain( el => el.Key == elementToDelete );
                    new ResourceFile( args.TemporaryFiles[1] ).Elements.Should().Contain( el => el.Key == elementToDelete );
                } )
                .ValidateRun( args =>
                {
                    new ResourceFile( args.TemporaryFiles[0] ).Elements.Should().NotContain( el => el.Key == elementToDelete );
                    new ResourceFile( args.TemporaryFiles[1] ).Elements.Should().NotContain( el => el.Key == elementToDelete );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput[0].Should().Be( $"{args.TemporaryFiles[0]}: '{elementToDelete}' have been removed" );
                    args.ConsoleOutput[1].Should().Be( $"{args.TemporaryFiles[1]}: '{elementToDelete}' have been removed" );
                } );
        }

        [TestCase( @"remove [TmpFile] [TmpFile] -k [UniqueKey] [UniqueKey]" )]
        [TestCase( @"remove [TmpFile] [TmpFile] --key [UniqueKey] [UniqueKey]" )]
        [TestCase( @"remove -s [TmpFile] [TmpFile] -k [UniqueKey] [UniqueKey]" )]
        [TestCase( @"remove --source [TmpFile] [TmpFile] --key [UniqueKey] [UniqueKey]" )]
        public void RemoveElementsByKey( string commandLine )
        {
            var resTemplate = GetExampleResourceFile();
            var elementsToDelete = new []{resTemplate.Elements[0].Key, resTemplate.Elements[2].Key};

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { elementsToDelete[0], elementsToDelete[1] } } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( args =>
                {
                    var res1 = new ResourceFile( args.TemporaryFiles[0] );
                    res1.Elements.Should().Contain( el => el.Key == elementsToDelete[0] );
                    res1.Elements.Should().Contain( el => el.Key == elementsToDelete[1] );

                    var res2 = new ResourceFile( args.TemporaryFiles[1] );
                    res2.Elements.Should().Contain( el => el.Key == elementsToDelete[0] );
                    res2.Elements.Should().Contain( el => el.Key == elementsToDelete[1] );
                } )
                .ValidateRun( args =>
                {
                    var res1 = new ResourceFile( args.TemporaryFiles[0] );
                    res1.Elements.Should().NotContain( el => el.Key == elementsToDelete[0] );
                    res1.Elements.Should().NotContain( el => el.Key == elementsToDelete[1] );

                    var res2 = new ResourceFile( args.TemporaryFiles[1] );
                    res2.Elements.Should().NotContain( el => el.Key == elementsToDelete[0] );
                    res2.Elements.Should().NotContain( el => el.Key == elementsToDelete[1] );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"{args.TemporaryFiles[0]}: '{elementsToDelete[0]}' have been removed",
                        $"{args.TemporaryFiles[0]}: '{elementsToDelete[1]}' have been removed",
                        $"{args.TemporaryFiles[1]}: '{elementsToDelete[0]}' have been removed",
                        $"{args.TemporaryFiles[1]}: '{elementsToDelete[1]}' have been removed" );
                } );
        }

        #endregion

        [TestCase( @"remove [TmpFile] [TmpFile] --empty" )]
        [TestCase( @"remove [TmpFile] [TmpFile] --empty-value" )]
        [TestCase( @"remove -s [TmpFile] [TmpFile] --empty" )]
        [TestCase( @"remove --source [TmpFile] [TmpFile] --empty-value" )]
        public void RemoveEmptyElements( string commandLine )
        {
            var resTemplate = new ResourceFile( ResourceFormatType.Resx );
            resTemplate.Elements.Add( "Key1", "Value1" );
            resTemplate.Elements.Add( "Key2", "" );
            resTemplate.Elements.Add( "Key3", "Value3" );
            resTemplate.Elements.Add( "Key4", "" );
            resTemplate.Elements.Add( "Key5", "Value5" );
            var path1 = GetOutputPath( UniqueKey() );
            var path2 = GetOutputPath( UniqueKey() );
            resTemplate.Save( path1 );
            resTemplate.Save( path2 );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { TemporaryFiles = { path1, path2 } } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( () =>
                {
                    new ResourceFile( path1 )
                        .Elements.Select( el => el.Key )
                        .Should().BeEquivalentTo( resTemplate.Elements.Select( el => el.Key ) );
                    new ResourceFile( path2 )
                        .Elements.Select( el => el.Key )
                        .Should().BeEquivalentTo( resTemplate.Elements.Select( el => el.Key ) );
                } )
                .ValidateRun( () =>
                {
                    new ResourceFile( path1 )
                        .Elements.Select( el => el.Key )
                        .Should().BeEquivalentTo( resTemplate.Elements[0].Key, resTemplate.Elements[2].Key, resTemplate.Elements[4].Key );
                    new ResourceFile( path2 )
                        .Elements.Select( el => el.Key )
                        .Should().BeEquivalentTo( resTemplate.Elements[0].Key, resTemplate.Elements[2].Key, resTemplate.Elements[4].Key );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"{path1}: '{resTemplate.Elements[1].Key}' have been removed",
                        $"{path1}: '{resTemplate.Elements[3].Key}' have been removed",
                        $"{path2}: '{resTemplate.Elements[1].Key}' have been removed",
                        $"{path2}: '{resTemplate.Elements[3].Key}' have been removed" );
                } );
        }
    }
}