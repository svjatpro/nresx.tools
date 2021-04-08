using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Helpers;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Convert
{
    [TestFixture]
    public class ConvertSingleResourceTests : TestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase( @"convert --source [Files]\Resources.resw --destination [Output]\[UniqueKey].yml --format yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [Files]\Resources.resw --destination [Output]\[UniqueKey].yml --format yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [Files]\Resources.resw -d [Output]\[UniqueKey].yml -f yml", ResourceFormatType.Yml )]
        [TestCase( @"convert -s [Files]\Resources.yaml -d [Output]\[UniqueKey].resx -f resx", ResourceFormatType.Resx )]
        public void ConvertFile( string commandLine, ResourceFormatType type )
        {
            Run( commandLine, out var fileName );
            var outputPath = GetOutputPath( fileName, type );

            var res = new ResourceFile( outputPath );
            res.ResourceFormat.Should().Be( type );
            ValidateElements( res );
        }

        [TestCase( @"convert --source [Files]\Resources.resw --destination [Output]\[UniqueKey].yaml", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [Files]\Resources.resw --destination [Output]\[UniqueKey].yaml", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [Files]\Resources.resw -d [Output]\[UniqueKey].yaml", ResourceFormatType.Yaml )]
        [TestCase( @"convert -s [Files]\Resources.yaml -d [Output]\[UniqueKey].resx", ResourceFormatType.Resx )]
        public void ConverFileByDestinationExtension( string commandLine, ResourceFormatType type )
        {
            Run( commandLine, out var fileName );
            var outputPath = GetOutputPath( fileName, type );

            var res = new ResourceFile( outputPath );
            res.ResourceFormat.Should().Be( type );
            ValidateElements( res );
        }

        [TestCase( ResourceFormatType.Resx, ResourceFormatType.Yaml )]
        [TestCase( ResourceFormatType.Yaml, ResourceFormatType.Resx )]
        public void ConverFileWithTheSamePath( ResourceFormatType sourceType, ResourceFormatType destType )
        {
            // prepare source file
            var sourceFile = Path.ChangeExtension( $"{OutputFolder}\\{UniqueKey()}", ResourceFormatHelper.GetExtension( sourceType ) );
            var source = GetExampleResourceFile();
            source.Save( sourceFile );

            // if there is no destination, then file will be converted to the same path, but with new format/extension
            var cmdLine = $"convert -s {sourceFile} -f {OptionHelper.GetFormatOption( destType )}";
            Run( cmdLine );
            
            var outputPath = Path.ChangeExtension( sourceFile, OptionHelper.GetFormatOption( destType ) );

            var res = new ResourceFile( outputPath );
            res.ResourceFormat.Should().Be( destType );
            ValidateElements( res );
        }

        [TestCase( @"convert --source [Files]\Resources.resw --destination [Output]\[UniqueKey].resw", ResourceFormatType.Resw )]
        [TestCase( @"convert -s [Files]\Resources.yaml -d [Output]\[UniqueKey].yaml", ResourceFormatType.Yaml )]
        public void CopyFileForTheSameFormat( string commandLine, ResourceFormatType type )
        {
            Run( commandLine, out var fileName );
            var outputPath = GetOutputPath( fileName, type );

            var res = new ResourceFile( outputPath );
            res.ResourceFormat.Should().Be( type );
            ValidateElements( res );
        }
    }
}