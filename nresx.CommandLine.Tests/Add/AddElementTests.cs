using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Add
{
    [TestFixture]
    public class AddElementTests : TestBase
    {
        [TestCase( @"add [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"add [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"add [TmpFile] --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add [TmpFile] --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void AddSingleElement( string commandLine )
        {
            var args = Run( commandLine );
            
            var file = args.TemporaryFiles[0];
            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];
            var comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : string.Empty;
           
            var res = new ResourceFile( file );
            res.Elements.Should().Contain( el => 
                el.Key == key && el.Value == value && el.Comment == comment );
        }

        [TestCase( @"add [Output]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add -s [Output]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey]" )]
        public void AddSingleElementToNonExistingResource( string commandLine )
        {
            var args = Run( commandLine );

            //args.ExitCode.Should().NotBe( 0 );
            var nonExistingFile = $"{GetOutputPath( args.UniqueKeys[0], ResourceFormatType.Resx )}";
            new FileInfo( nonExistingFile ).Exists.Should().BeFalse();

            args.ConsoleOutput[0].Should().Be( $"fatal: path mask '{nonExistingFile}' did not match any files" );
        }

        [TestCase( @"add [Output]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] --new" )]
        [TestCase( @"add [Output]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --new" )]
        public void AddSingleElementToNonExistingResourceShoulsCreateTheResource( string commandLine )
        {
            var args = Run( commandLine );

            args.ExitCode.Should().Be( 0 );
            
            var newFile = $"{GetOutputPath( args.UniqueKeys[0], ResourceFormatType.Resx )}";
            var key = args.UniqueKeys[1];
            var value = args.UniqueKeys[2];
            var res = new ResourceFile( newFile );
            res.Elements.Should().ContainSingle( el => el.Key == key && el.Value == value );
        }

        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] --new" )]
        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --new" )]
        public void AddSingleElementToNonExistingResourceShoulsNotCreateNonexistingDirectory( string commandLine )
        {
            var args = Run( commandLine );

            //args.ExitCode.Should().Be( 0 );

            var newFile = $"{GetOutputPath( $"{args.UniqueKeys[0]}\\{args.UniqueKeys[1]}", ResourceFormatType.Resx )}";
            new FileInfo( newFile ).Exists.Should().BeFalse();

            args.ConsoleOutput[0].Should().Be( $"fatal: Invalid path: '{newFile}': no such file or directory" );
        }

        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] -n -r" )]
        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --new --recursive" )]
        public void AddSingleElementToNonExistingResourceShoulsCreateDirectory( string commandLine )
        {
            var args = Run( commandLine );

            //args.ExitCode.Should().Be( 0 );
            
            var newFile = $"{GetOutputPath( $"{args.UniqueKeys[0]}\\{args.UniqueKeys[1]}", ResourceFormatType.Resx )}";
            var key = args.UniqueKeys[2];
            var value = args.UniqueKeys[3];
            var res = new ResourceFile( newFile );
            res.Elements.Should().ContainSingle( el => el.Key == key && el.Value == value );
        }
    }
}