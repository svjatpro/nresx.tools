using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Remove
{
    [TestFixture]
    public class RemoveElementSingleResourceTests : TestBase
    {
        #region Remove element(s) by key(s)

        [TestCase( @"remove [TmpFile] -k [UniqueKey]" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey]" )]
        [TestCase( @"remove -s [TmpFile] -k [UniqueKey]" )]
        [TestCase( @"remove --source [TmpFile] --key [UniqueKey]" )]
        public void RemoveSingleElementByKey( string commandLine )
        {
            var resTemplate = GetExampleResourceFile();
            var elementToDelete = resTemplate.Elements.First();
            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { elementToDelete.Key }} );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().NotContain( el => el.Key == elementToDelete.Key );
        }

        [TestCase( @"remove [TmpFile] -k [UniqueKey] --dry-run" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey] --dry-run" )]
        [TestCase( @"remove -s [TmpFile] -k [UniqueKey] --dry-run" )]
        [TestCase( @"remove --source [TmpFile] --key [UniqueKey] --dry-run" )]
        public void RemoveSingleElementByKeyDryRun( string commandLine )
        {
            var resTemplate = GetExampleResourceFile();
            var elementToDelete = resTemplate.Elements.First();
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementToDelete.Key } } );

            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().Contain( el => el.Key == elementToDelete.Key );

            args.ConsoleOutput.Single().Should().Be( $"{args.TemporaryFiles[0]}: '{elementToDelete.Key}' have been removed" );
        }

        [TestCase( @"remove [TmpFile] -k [UniqueKey] [UniqueKey]" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey] [UniqueKey]" )]
        [TestCase( @"remove -s [TmpFile] -k [UniqueKey] [UniqueKey]" )]
        [TestCase( @"remove --source [TmpFile] --key [UniqueKey] [UniqueKey]" )]
        public void RemoveElementsByKey( string commandLine )
        {
            var resTemplate = GetExampleResourceFile();
            var elementsToDelete = new []{resTemplate.Elements[0].Key, resTemplate.Elements[2].Key};
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementsToDelete[0], elementsToDelete[1] } } );

            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().NotContain( el => el.Key == elementsToDelete[0] );
            res.Elements.Should().NotContain( el => el.Key == elementsToDelete[1] );
        }

        [TestCase( @"remove [TmpFile] -k [UniqueKey] [UniqueKey] --dry-run" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey] [UniqueKey] --dry-run" )]
        [TestCase( @"remove -s [TmpFile] -k [UniqueKey] [UniqueKey] --dry-run" )]
        [TestCase( @"remove --source [TmpFile] --key [UniqueKey] [UniqueKey] --dry-run" )]
        public void RemoveElementsByKeyDryRun( string commandLine )
        {
            var resTemplate = GetExampleResourceFile();
            var elementsToDelete = new[] { resTemplate.Elements[0].Key, resTemplate.Elements[2].Key };
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementsToDelete[0], elementsToDelete[1] } } );

            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().Contain( el => el.Key == elementsToDelete[0] );
            res.Elements.Should().Contain( el => el.Key == elementsToDelete[1] );

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"{args.TemporaryFiles[0]}: '{elementsToDelete[0]}' have been removed",
                $"{args.TemporaryFiles[0]}: '{elementsToDelete[1]}' have been removed" );
        }

        #endregion

        [TestCase( @"remove [TmpFile] --empty" )]
        [TestCase( @"remove [TmpFile] --empty-value" )]
        [TestCase( @"remove -s [TmpFile] --empty" )]
        [TestCase( @"remove --source [TmpFile] --empty-value" )]
        public void RemoveEmptyElements( string commandLine )
        {
            var resTemplate = new ResourceFile( ResourceFormatType.Resx );
            resTemplate.Elements.Add( "Key1", "Value1" );
            resTemplate.Elements.Add( "Key2", "" );
            resTemplate.Elements.Add( "Key3", "Value3" );
            resTemplate.Elements.Add( "Key4", "" );
            resTemplate.Elements.Add( "Key5", "Value5" );
            var path = GetOutputPath( UniqueKey() );
            resTemplate.Save( path );

            Run( commandLine, new CommandLineParameters { TemporaryFiles = { path } } );

            new ResourceFile( path )
                .Elements.Select( el => el.Key )
                .Should().BeEquivalentTo( 
                    resTemplate.Elements[0].Key, 
                    resTemplate.Elements[2].Key, 
                    resTemplate.Elements[4].Key );
        }

        [TestCase( @"remove [TmpFile] --empty --dry-run" )]
        [TestCase( @"remove [TmpFile] --empty-value  --dry-run" )]
        [TestCase( @"remove -s [TmpFile] --empty --dry-run" )]
        [TestCase( @"remove --source [TmpFile] --empty-value --dry-run" )]
        public void RemoveEmptyElementsDryRun( string commandLine )
        {
            var resTemplate = new ResourceFile( ResourceFormatType.Resx );
            resTemplate.Elements.Add( "Key1", "Value1" );
            resTemplate.Elements.Add( "Key2", "" );
            resTemplate.Elements.Add( "Key3", "Value3" );
            resTemplate.Elements.Add( "Key4", "" );
            resTemplate.Elements.Add( "Key5", "Value5" );
            var path = GetOutputPath( UniqueKey() );
            resTemplate.Save( path );

            var args = Run( commandLine, new CommandLineParameters { TemporaryFiles = { path } } );

            new ResourceFile( path )
                .Elements.Select( el => el.Key )
                .Should().BeEquivalentTo( resTemplate.Elements.Select( el => el.Key ) );

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"{path}: '{resTemplate.Elements[1].Key}' have been removed",
                $"{path}: '{resTemplate.Elements[3].Key}' have been removed"  );
        }
    }
}