using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.ElementManagement
{
    [TestFixture]
    public class RemoveElementTests : TestBase
    {
        [TestCase( @"remove [TmpFile] -k [UniqueKey]" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey]" )]
        public void RemoveSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToDelete = res1.Elements.First();
            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { elementToDelete.Key }} );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().NotContain( el => el.Key == elementToDelete.Key );
        }

        [TestCase( @"remove [TmpFile] -k [UniqueKey] --dry-run" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey] --dry-run" )]
        public void RemoveSingleElementDryRun( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToDelete = res1.Elements.First();
            var args = Run( commandLine, new CommandLineParameters { UniqueKeys = { elementToDelete.Key } } );

            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().Contain( el => el.Key == elementToDelete.Key );

            args.ConsoleOutput.Single().Should().Be( $"{args.TemporaryFiles[0]}: '{elementToDelete.Key}' have been removed" );
        }

        [TestCase( @"remove [TmpFile] --empty" )]
        [TestCase( @"remove [TmpFile] --empty-value" )]
        [TestCase( @"remove -s [TmpFile] --empty" )]
        [TestCase( @"remove --source [TmpFile] --empty-value" )]
        public void RemoveEmptyElements( string commandLine )
        {
            var res1 = new ResourceFile( ResourceFormatType.Resx );
            res1.Elements.Add( "Key1", "Value1" );
            res1.Elements.Add( "Key2", "" );
            res1.Elements.Add( "Key3", "Value3" );
            res1.Elements.Add( "Key4", "" );
            res1.Elements.Add( "Key5", "Value5" );
            var path = GetOutputPath( UniqueKey() );
            res1.Save( path );

            var args = Run( commandLine, new CommandLineParameters { TemporaryFiles = { path } } );

            var res = new ResourceFile( path );
            res.Elements.Select( el => el.Key )
                .Should().BeEquivalentTo( 
                    res1.Elements[0].Key, 
                    res1.Elements[2].Key, 
                    res1.Elements[4].Key );
        }

        [TestCase( @"remove [TmpFile] --empty --dry-run" )]
        [TestCase( @"remove [TmpFile] --empty-value  --dry-run" )]
        [TestCase( @"remove -s [TmpFile] --empty --dry-run" )]
        [TestCase( @"remove --source [TmpFile] --empty-value --dry-run" )]
        public void RemoveEmptyElementsDryRun( string commandLine )
        {
            var res1 = new ResourceFile( ResourceFormatType.Resx );
            res1.Elements.Add( "Key1", "Value1" );
            res1.Elements.Add( "Key2", "" );
            res1.Elements.Add( "Key3", "Value3" );
            res1.Elements.Add( "Key4", "" );
            res1.Elements.Add( "Key5", "Value5" );
            var path = GetOutputPath( UniqueKey() );
            res1.Save( path );

            var args = Run( commandLine, new CommandLineParameters { TemporaryFiles = { path } } );

            var res = new ResourceFile( path );
            res.Elements.Select( el => el.Key )
                .Should().BeEquivalentTo(
                    res1.Elements[0].Key,
                    res1.Elements[1].Key,
                    res1.Elements[2].Key,
                    res1.Elements[3].Key,
                    res1.Elements[4].Key );

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"{path}: '{res1.Elements[3].Key}' have been removed",
                $"{path}: '{res1.Elements[1].Key}' have been removed"  );
        }
    }
}