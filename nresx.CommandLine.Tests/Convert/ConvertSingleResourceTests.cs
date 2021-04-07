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
        [TestCase( @"convert -s [Files]\Resources.yaml -d [Output]\[UniqueKey].resx", ResourceFormatType.Yaml )]
        public void ConverFileByDestinationExtension( string commandLine, ResourceFormatType type )
        {
            Run( commandLine, out var fileName );
            var outputPath = GetOutputPath( fileName, type );

            var res = new ResourceFile( outputPath );
            res.ResourceFormat.Should().Be( type );
            ValidateElements( res );
        }
    }
}