using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Add
{
    [TestFixture]
    public class AddElementSingleResourceTests : TestBase
    {
        [TestCase( @"add [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"add [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"add [TmpFile] --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add [TmpFile] --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void AddSingleElement( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .WithParams( args => new
                {
                    file = args.TemporaryFiles[0],
                    key = args.UniqueKeys[0],
                    value = args.UniqueKeys[1],
                    comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : null
                } )
                .ValidateDryRun( ( _, param ) =>
                {
                    var res = new ResourceFile( param.file );
                    res.Elements.Should().NotContain( el =>
                        el.Key == param.key &&
                        el.Value == param.value &&
                        ( param.comment == null || !res.ElementHasComment || el.Comment == param.comment ) );
                } )
                .ValidateRun( ( _, param ) =>
                {
                    var res = new ResourceFile( param.file );
                    res.Elements.Should().Contain( el =>
                        el.Key == param.key &&
                        el.Value == param.value &&
                        ( param.comment == null || !res.ElementHasComment || el.Comment == param.comment ) );
                } )
                .ValidateStdout( (args, param) => 
                    args.ConsoleOutput.Should().BeEquivalentTo( $"'{param.key}: {param.value}' element have been add to '{param.file}'" ) );
        }


        [TestCase( @"add [NewFile] --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add -s [NewFile] -k [UniqueKey] -v [UniqueKey]" )]
        public void AddSingleElementToNonExistingResource( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .WithParams( args => new { file = args.NewFiles[0].GetShortPath() } )
                .ValidateDryRun( ( _, param ) =>
                {
                    new FileInfo( param.file ).Exists.Should().BeFalse();
                } )
                .ValidateRun( ( _, param ) =>
                {
                    new FileInfo( param.file ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput[0].Should().Be( $"fatal: path mask '{param.file}' did not match any files" );
                } );
        }



        [TestCase( @"add [NewFile] -k [UniqueKey] -v [UniqueKey] --new-file" )]
        [TestCase( @"add [NewFile] --key [UniqueKey] --value [UniqueKey] --new-file" )]
        public void AddSingleElementToNonExistingResourceShouldCreateTheResource( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .WithParams( args => new
                { 
                    file = args.NewFiles[0].GetShortPath(), 
                    key = args.UniqueKeys[0], 
                    value = args.UniqueKeys[1]
                } )
                .ValidateDryRun( ( args, param ) =>
                {
                    args.ExitCode.Should().Be( 0 );
                    new FileInfo( param.file ).Exists.Should().BeFalse();
                } )
                .ValidateRun( ( args, param ) =>
                {
                    args.ExitCode.Should().Be( 0 );
                    var res = new ResourceFile( param.file );
                    res.Elements.Should().ContainSingle( el => el.Key == param.key && el.Value == param.value );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should()
                        .BeEquivalentTo( $"'{param.key}: {param.value}' element have been add to '{param.file}'" );
                } );
        }

        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey][Ext] -k [UniqueKey] -v [UniqueKey] --new-file" )]
        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey][Ext] --key [UniqueKey] --value [UniqueKey] --new-file" )]
        public void AddSingleElementToNonExistingResourceShouldNotCreateNonExistingDirectory( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .WithParams( args => new { file = $"{GetOutputPath( $"{args.UniqueKeys[0]}\\{args.UniqueKeys[1]}{args.RandomExtensions[0]}" )}" } )
                .ValidateDryRun( ( _, param ) =>
                {
                    new FileInfo( param.file ).Exists.Should().BeFalse();
                } )
                .ValidateRun( ( _, param ) =>
                {
                    new FileInfo( param.file ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput[0].Should().Be( $"fatal: Invalid path: '{param.file}': no such file or directory" );
                } );
        }

        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey][Ext] -k [UniqueKey] -v [UniqueKey] --new-file -r" )]
        [TestCase( @"add [Output]\[UniqueKey]\[UniqueKey][Ext] --key [UniqueKey] --value [UniqueKey] --new-file --recursive" )]
        public void AddSingleElementToNonExistingResourceShouldCreateDirectory( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .WithParams( args => new
                {
                    file = $"{GetOutputPath( $"{args.UniqueKeys[0]}\\{args.UniqueKeys[1]}{args.RandomExtensions[0]}" )}",
                    key = args.UniqueKeys[2],
                    value = args.UniqueKeys[3]
                } )
                .ValidateDryRun( ( _, param ) =>
                {
                    new FileInfo( param.file ).Exists.Should().BeFalse();
                } )
                .ValidateRun( ( _, param ) =>
                {
                    var res = new ResourceFile( param.file );
                    res.Elements.Should().ContainSingle( el => el.Key == param.key && el.Value == param.value );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"'{param.key}: {param.value}' element have been add to '{param.file.GetShortPath()}'" );
                } );
        }
    }
}