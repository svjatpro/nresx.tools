using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Convert
{
    [TestFixture]
    public class ConvertMany2ManyResourceTests : ConvertBasicTests
    {
        [TestCase( @"convert -s [TmpFile.resx] [TmpFile.resw] -f yaml" )]
        [TestCase( @"convert -s [Dir\TmpFile.resx] [Dir\TmpFile.resw] -f yaml" )]
        [TestCase( @"convert --source [TmpFile.resx] [TmpFile.resw] --format yaml" )]
        [TestCase( @"convert --source [Dir\TmpFile.resx] [Dir\TmpFile.resw] --format yaml" )]
        public void ConvertFileToTheSameForder( string commandLine )
        {
            commandLine
                .ValidateRun( args =>
                {
                    args.TemporaryFiles.Should().HaveCount( 2 );
                    args.TemporaryFiles.ForEach( file =>
                    {
                        var res = new ResourceFile( Path.ChangeExtension( file, ".yaml" ) );
                        res.FileFormat.Should().Be( ResourceFormatType.Yaml );
                        ValidateElements( res );
                    } );
                } )
                .ValidateDryRun( args =>
                {
                    new FileInfo( Path.ChangeExtension( args.TemporaryFiles[0], "yaml" ) ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( args.TemporaryFiles[1], "yaml" ) ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( args => new[]
                {
                    string.Format( SuccessLineTemplate, args.TemporaryFiles[0], Path.ChangeExtension( args.TemporaryFiles[0], "yaml" ) ),
                    string.Format( SuccessLineTemplate, args.TemporaryFiles[1], Path.ChangeExtension( args.TemporaryFiles[1], "yaml" ) ),
                } );
        }


        [TestCase( @"convert [Output]\[UniqueKey]* -f yaml" )]
        [TestCase( @"convert [Output]\[UniqueKey]*.* -f yaml" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]*.* -f yaml" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]*.* -f yaml" )]

        [TestCase( @"convert [Output]\[UniqueKey]*.* *.yaml" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]*.* -d *.yaml" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]*.* --destination *.yaml" )]
        public void ConvertFilesByPathSpec( string commandLine )
        {
            var files1 = PrepareTemporaryFiles( 2, 2, out var key1 );

            commandLine
                .PrepareArgs( () => new CommandLineParameters {UniqueKeys = {key1}} )
                .ValidateDryRun( args => // in this case order is important, because we use the same predefined files for both run
                {
                    new FileInfo( Path.ChangeExtension( files1[0], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[1], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[2], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[3], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                } )
                .ValidateRun( args =>
                {
                    ValidateElements( new ResourceFile( Path.ChangeExtension( files1[0], "yaml" ) ) );
                    ValidateElements( new ResourceFile( Path.ChangeExtension( files1[1], "yaml" ) ) );
                    new FileInfo( Path.ChangeExtension( files1[2], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[3], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( args => new[]
                {
                    string.Format( SuccessLineTemplate, files1[0], Path.ChangeExtension( files1[0], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[1], Path.ChangeExtension( files1[1], "yaml" ) )
                } );
        }

        [TestCase( @"convert [UniqueKey]* -f yaml -r" )]
        [TestCase( @"convert [Output]\[UniqueKey]* -f yaml -r" )]
        [TestCase( @"convert [Output]\[UniqueKey]*.* -f yaml -r" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]*.* -f yaml -r" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]*.* -f yaml --recursive" )]

        [TestCase( @"convert [Output]\[UniqueKey]*.* *.yaml -r" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]*.* -d *.yaml -r" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]*.* --destination *.yaml --recursive" )]
        public void ConvertFilesByPathSpecRecursive( string commandLine )
        {
            var files1 = PrepareTemporaryFiles( 2, 2, out var key1 );

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1 } } )
                .ValidateDryRun( args => // in this case order is important, because we use the same predefined files for both run
                {
                    new FileInfo( Path.ChangeExtension( files1[0], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[1], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[2], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                    new FileInfo( Path.ChangeExtension( files1[3], "yaml" ) ?? string.Empty ).Exists.Should().BeFalse();
                } )
                .ValidateRun( args =>
                {
                    ValidateElements( new ResourceFile( Path.ChangeExtension( files1[0], "yaml" ) ) );
                    ValidateElements( new ResourceFile( Path.ChangeExtension( files1[1], "yaml" ) ) );
                    ValidateElements( new ResourceFile( Path.ChangeExtension( files1[2], "yaml" ) ) );
                    ValidateElements( new ResourceFile( Path.ChangeExtension( files1[3], "yaml" ) ) );
                } )
                .ValidateStdout( args => new[]
                {
                    string.Format( SuccessLineTemplate, files1[0], Path.ChangeExtension( files1[0], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[1], Path.ChangeExtension( files1[1], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[2], Path.ChangeExtension( files1[2], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[3], Path.ChangeExtension( files1[3], "yaml" ) )
                } );
        }

        [TestCase( @"convert [Output]\[UniqueKey]* [Output]\[UniqueKey]\*.yaml" )]
        [TestCase( @"convert [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\*.yaml" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\*.yaml" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]* --destination [Output]\[UniqueKey]\*.yaml" )]

        [TestCase( @"convert [Output]\[UniqueKey]* [Output]\[UniqueKey]\* -f yaml" )]
        [TestCase( @"convert [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\* -f yaml" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\* -f yaml" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]* --destination [Output]\[UniqueKey]\* --format yaml" )]
        public void ConvertFilesByPathSpecToAnotherDir( string commandLine )
        {
            var files1 = PrepareTemporaryFiles( 2, 2, out var key1 );

            commandLine
                .WithParams( args => new { NewDir = Path.Combine( TestData.OutputFolder, args.UniqueKeys[1] ) } )
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1 } } )
                .ValidateDryRun( (args, parameters) => // in this case order is important, because we use the same predefined files for both run
                {
                    for ( var i = 0; i < 4; i++ )
                    {
                        var newPath = Path.Combine( parameters.NewDir, Path.ChangeExtension( Path.GetFileName( files1[i] ), "yaml" ) ?? string.Empty );
                        new FileInfo( newPath ).Exists.Should().BeFalse();
                    }
                } )
                .ValidateRun( ( args, parameters ) =>
                {
                    for ( var i = 0; i < 2; i++ )
                    {
                        var newPath = new ResourceFile( Path.Combine( parameters.NewDir, Path.ChangeExtension( Path.GetFileName( files1[i] ), "yaml" ) ?? string.Empty ) );
                        ValidateElements( newPath );
                    }

                    for ( var i = 2; i < 4; i++ )
                    {
                        var newPath = Path.Combine( parameters.NewDir, Path.ChangeExtension( Path.GetFileName( files1[i] ), "yaml" ) ?? string.Empty );
                        new FileInfo( newPath ).Exists.Should().BeFalse();
                    }
                } )
                .ValidateStdout( ( args, parameters ) => new[]
                {
                    string.Format( SuccessLineTemplate, files1[0], Path.ChangeExtension( files1[0], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[1], Path.ChangeExtension( files1[1], "yaml" ) ),
                } );
        }

        [TestCase( @"convert [Output]\[UniqueKey]* [Output]\[UniqueKey]\*.yaml -r" )]
        [TestCase( @"convert [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\*.yaml -r" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\*.yaml -r" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]* --destination [Output]\[UniqueKey]\*.yaml -r" )]

        [TestCase( @"convert [Output]\[UniqueKey]* [Output]\[UniqueKey]\* -f yaml -r" )]
        [TestCase( @"convert [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\* -f yaml -r" )]
        [TestCase( @"convert -s [Output]\[UniqueKey]* -d [Output]\[UniqueKey]\* -f yaml -r" )]
        [TestCase( @"convert --source [Output]\[UniqueKey]* --destination [Output]\[UniqueKey]\* --format yaml --recursive" )]
        public void ConvertFilesByPathSpecToAnotherDirRecuresive( string commandLine )
        {
            var files1 = PrepareTemporaryFiles( 2, 2, out var key1 );

            commandLine
                .WithParams( args => new { NewDir = Path.Combine( TestData.OutputFolder, args.UniqueKeys[1] ) } )
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1 } } )
                .ValidateDryRun( ( args, parameters ) => // in this case order is important, because we use the same predefined files for both run
                {
                    for ( var i = 0; i < 4; i++ )
                    {
                        var newPath = Path.Combine( parameters.NewDir, Path.ChangeExtension( Path.GetFileName( files1[i] ), "yaml" ) ?? string.Empty );
                        new FileInfo( newPath ).Exists.Should().BeFalse();
                    }
                } )
                .ValidateRun( ( args, parameters ) =>
                {
                    for ( var i = 0; i < 4; i++ )
                    {
                        var newPath = new ResourceFile( Path.Combine( parameters.NewDir, Path.ChangeExtension( Path.GetFileName( files1[i] ), "yaml" ) ?? string.Empty ) );
                        ValidateElements( newPath );
                    }
                } )
                .ValidateStdout( ( args, parameters ) => new[]
                {
                    string.Format( SuccessLineTemplate, files1[0], Path.ChangeExtension( files1[0], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[1], Path.ChangeExtension( files1[1], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[2], Path.ChangeExtension( files1[2], "yaml" ) ),
                    string.Format( SuccessLineTemplate, files1[3], Path.ChangeExtension( files1[3], "yaml" ) )
                } );
        }
    }
}