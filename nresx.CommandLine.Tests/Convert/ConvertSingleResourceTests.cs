using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Convert
{
    [TestFixture]
    public class ConvertSingleResourceTests : TestBase
    {
        [TestCase( @"convert --source [SourceFile.resw] --destination [DestFile.yml] --format yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [SourceFile.resw] --destination [DestFile.yml] --format yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [SourceFile.resw] -d [DestFile.yml] -f yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [SourceFile.yaml] -d [DestFile.resx] -f resx", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] [DestFile.resx] -f resx", ResourceFormatType.Resx )]
        public void ConvertFile( string commandLine, ResourceFormatType type )
        {
            var args = Run( commandLine );

            var res = new ResourceFile( args.DestinationFiles[0] );
            res.FileFormat.Should().Be( type );
            ValidateElements( res );
        }

        [TestCase( @"convert --source [SourceFile.resw] --destination [DestFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.resw] --destination [DestFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.resw] -d [DestFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.yaml] -d [DestFile.resx]", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] [DestFile.resx]", ResourceFormatType.Resx )]
        [TestCase( @"convert [SourceFile.yaml] -d [DestFile.resx]", ResourceFormatType.Resx )]
        [TestCase( @"convert -s [SourceFile.yaml] [DestFile.resx]", ResourceFormatType.Resx )]
        public void ConverFileByDestinationExtension( string commandLine, ResourceFormatType type )
        {
            var args = Run( commandLine );

            var res = new ResourceFile( args.DestinationFiles[0] );
            res.FileFormat.Should().Be( type );
            ValidateElements( res );
        }

        [TestCase( ResourceFormatType.Resx, ResourceFormatType.Yaml )]
        [TestCase( ResourceFormatType.Yaml, ResourceFormatType.Resx )]
        public void ConverFileWithTheSamePath( ResourceFormatType sourceType, ResourceFormatType destType )
        {
            var sourceFile = CopyTemporaryFile( copyType: sourceType );

            // if there is no destination, then file will be converted to the same path, but with new format/extension
            var cmdLine = $"convert {sourceFile} -f {OptionHelper.GetFormatOption( destType )}";
            Run( cmdLine );
            
            var outputPath = Path.ChangeExtension( sourceFile, OptionHelper.GetFormatOption( destType ) );

            var res = new ResourceFile( outputPath );
            res.FileFormat.Should().Be( destType );
            ValidateElements( res );
        }

        [TestCase( @"convert --source [SourceFile.resw] --destination [DestFile.resw]", ResourceFormatType.Resw )]
        [TestCase( @"convert -s [SourceFile.yaml] -d [DestFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert [SourceFile.yaml] -d [DestFile.yaml]", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [SourceFile.yaml] [DestFile.yaml]", ResourceFormatType.Yaml )]
        public void CopyFileForTheSameFormat( string commandLine, ResourceFormatType type )
        {
            var args = Run( commandLine );

            var res = new ResourceFile( args.DestinationFiles[0] );
            res.FileFormat.Should().Be( type );
            ValidateElements( res );
        }
    }
}