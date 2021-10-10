using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Convert
{
    [TestFixture]
    public class ConvertSingleResourceTests : ConvertBasicTests
    {
        // basic syntax
        [TestCase( @"convert --source [SourceFile.resw] --destination [NewFile.yml] --format yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [SourceFile.resw] --destination [NewFile.yml] --format yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [SourceFile.resw] -d [NewFile.yml] -f yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [SourceFile.yaml] -d [NewFile.resx] -f resx", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] -d [NewFile.resx] -f resx", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] [NewFile.resx] -f resx", ResourceFormatType.Resx )]

        // detect dest format by extension
        [TestCase( @"convert --source [SourceFile.resw] --destination [NewFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.resw] --destination [NewFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.resw] -d [NewFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.yaml] -d [NewFile.resx]", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] [NewFile.resx]", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] -d [NewFile.resx]", ResourceFormatType.Resx )]

        // another file, the same format
        [TestCase( @"convert --source [SourceFile.resw] --destination [NewFile.resw]", ResourceFormatType.Resw )]
        [TestCase( @"convert -s [SourceFile.yaml] -d [NewFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert [SourceFile.yaml] -d [NewFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert [SourceFile.yaml] [NewFile.yaml]", ResourceFormatType.Yaml )]
        public void ConvertSingleFile( string commandLine, ResourceFormatType type )
        {
            commandLine
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.FileFormat.Should().Be( type );
                    ValidateElements( res );
                } )
                .ValidateDryRun( args =>
                {
                    new FileInfo( args.NewFiles[0] ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( string.Format( SuccessLineTemplate, args.SourceFiles[0], args.NewFiles[0] ) );
                } );
        }

        [TestCase( ResourceFormatType.Resx, ResourceFormatType.Yaml )]
        [TestCase( ResourceFormatType.Yaml, ResourceFormatType.Resx )]
        public void ConvertFileWithTheSamePath( ResourceFormatType sourceType, ResourceFormatType destType )
        {
            // if there is no destination, then file will be converted to the same path, but with new format/extension
            var cmdLine = $"convert [SourceFile.{OptionHelper.GetFormatOption( sourceType )}] -f {OptionHelper.GetFormatOption( destType )}";
            
            cmdLine
                .PrepareArgs( () =>
                {
                    var sourceFile = TestHelper.CopyTemporaryFile( copyType: sourceType );
                    return new CommandLineParameters {SourceFiles = {sourceFile}};
                } )
                .WithParams( args => new { DestPath = Path.ChangeExtension( args.SourceFiles[0], OptionHelper.GetFormatOption( destType ) ) } )
                .ValidateRun( (args, parameters) =>
                {
                    var res = new ResourceFile( parameters.DestPath );
                    res.FileFormat.Should().Be( destType );
                    ValidateElements( res, sourceType );
                } )
                .ValidateDryRun( ( args, parameters ) =>
                {
                    new FileInfo( parameters.DestPath ).Exists.Should().BeFalse();
                } )
                .ValidateStdout( ( args, parameters ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( string.Format( SuccessLineTemplate, args.SourceFiles[0], parameters.DestPath ) );
                } );
        }
    }
}