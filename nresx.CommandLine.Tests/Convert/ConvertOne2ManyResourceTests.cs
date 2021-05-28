using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Convert
{
    [TestFixture]
    public class ConvertOne2ManyResourceTests : ConvertBasicTests
    {
        [TestCase( @"convert [SourceFile.resw] -d [NewFile.yaml] [NewFile.resx]" )]
        [TestCase( @"convert -s [SourceFile.resw] -d [NewFile.yaml] [NewFile.resx]" )]
        [TestCase( @"convert --source [SourceFile.resw] --destination [NewFile.yaml] [NewFile.resx]" )]
        
        [TestCase( @"convert [SourceFile.resw] -d [NewFile.yaml] [NewFile.resx] -f yaml" )]
        [TestCase( @"convert -s [SourceFile.resw] -d [NewFile.yaml] [NewFile.resx] -f yaml" )]
        [TestCase( @"convert --source [SourceFile.resw] --destination [NewFile.yaml] [NewFile.resx]  -f yaml" )]
        public void ConvertToSeveralFiles( string commandLine )
        {
            commandLine
                .ValidateRun( args =>
                {
                    args.NewFiles.Count.Should().Be( 2 );
                    args.NewFiles.ForEach( file =>
                    {
                        var res1 = new ResourceFile( file );
                        ValidateElements( res1 );

                        ResourceFormatHelper.DetectFormatByExtension( file, out var format );
                        res1.FileFormat.Should().Be( format );
                    } );
                } )
                .ValidateDryRun( args =>
                {
                    args.NewFiles.Count.Should().Be( 2 );
                    foreach ( var file in args.NewFiles )
                        new FileInfo( file ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( args => new[]
                {
                    string.Format( SuccessLineTemplate, args.SourceFiles[0], args.NewFiles[0] ),
                    string.Format( SuccessLineTemplate, args.SourceFiles[0], args.NewFiles[1] )
                } );
        }

        [TestCase( @"convert [TmpFile.resx] -f yaml" )]
        [TestCase( @"convert [TmpFile.resx] *.* -f yaml" )]
        [TestCase( @"convert [TmpFile.resx] *.yaml" )]
        [TestCase( @"convert [TmpFile.resx] -d *.yaml" )]
        [TestCase( @"convert -s [TmpFile.resx] -d *.yaml" )]
        [TestCase( @"convert --source [TmpFile.resx] --destination *.yaml" )]
        public void DetectDestinationNameBySource( string commandLine )
        {
            commandLine
                .WithParams( args => new { DestPath = Path.ChangeExtension( args.TemporaryFiles[0], "yaml" ) } )
                .ValidateRun( (args, parameters) =>
                {
                    var res1 = new ResourceFile( parameters.DestPath );
                    res1.FileFormat.Should().Be( ResourceFormatType.Yaml );
                    ValidateElements( res1 );
                } )
                .ValidateDryRun( ( args, parameters ) =>
                {
                    new FileInfo( parameters.DestPath ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( ( args, parameters ) => new[]
                {
                    string.Format( SuccessLineTemplate, args.TemporaryFiles[0], parameters.DestPath ),
                } );
        }

        [TestCase( @"convert [TmpFile.resx] [Output]\[UniqueKey].* -f resx" )]
        [TestCase( @"convert [TmpFile.resx] -d [Output]\[UniqueKey].* -f resx" )]
        [TestCase( @"convert -s [TmpFile.resx] -d [Output]\[UniqueKey].* -f resx" )]
        [TestCase( @"convert --source [TmpFile.resx] --destination [Output]\[UniqueKey].* -f resx" )]
        public void CopySourceFile( string commandLine )
        {
            commandLine
                .WithParams( args => new { DestPath = GetOutputPath( args.UniqueKeys[0] ) } )
                .ValidateRun( ( args, parameters ) =>
                {
                    var res1 = new ResourceFile( parameters.DestPath );
                    res1.FileFormat.Should().Be( ResourceFormatType.Resx );
                    ValidateElements( res1 );
                } )
                .ValidateDryRun( ( args, parameters ) =>
                {
                    new FileInfo( parameters.DestPath ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( ( args, parameters ) => new[]
                {
                    string.Format( SuccessLineTemplate, args.TemporaryFiles[0], parameters.DestPath ),
                } );
        }

        [TestCase( @"convert [TmpFile.resx] [Output]\[UniqueKey].*" )]
        [TestCase( @"convert [TmpFile.resx] -d [Output]\[UniqueKey].*" )]
        [TestCase( @"convert -s [TmpFile.resx] -d [Output]\[UniqueKey].*" )]
        [TestCase( @"convert --source [TmpFile.resx] --destination [Output]\[UniqueKey].*" )]
        public void CanNotDetectDestinationFormat( string commandLine )
        {
            commandLine
                .WithParams( args => new { DestPath = GetOutputPath( args.UniqueKeys[0] ) } )
                .ValidateRun( ( args, parameters ) => { new FileInfo( parameters.DestPath ).Exists.Should().BeFalse(); } )
                .ValidateDryRun( ( args, parameters ) => { new FileInfo( parameters.DestPath ).Exists.Should().BeFalse(); } )
                .ValidateStdout( ( args, parameters ) => new[] {FormatUndefinedErrorMessage} );
        }
    }
}