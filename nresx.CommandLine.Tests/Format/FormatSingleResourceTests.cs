using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Format
{
    [TestFixture]
    public class FormatSingleResourceTests : TestBase
    {
        [TestCase( @"format --source [TmpFile] --start-with --pattern prefix_", "prefix_" )]
        [TestCase( @"format -s [TmpFile] --start-with -p prefix_", "prefix_" )]
        [TestCase( @"format -s [TmpFile.resw] --start-with -p prefix_", "prefix_" )]
        [TestCase( @"format -s [TmpFile.Yaml] --start-with -p prefix_", "prefix_" )]
        public void AddPrefixToTheSameFile( string commandLine, string prefix )
        {
            var args = Run( commandLine );
            var res = new ResourceFile( args.TemporaryFiles[0] );

            res.Elements.Where( el => el.Value.StartsWith( prefix ) ).Should().HaveCount( res.Elements.Count() );
        }

        [TestCase( @"format --source [TmpFile] --delete --start-with --pattern prefix_", "prefix_" )]
        [TestCase( @"format -s [TmpFile] --delete --start-with -p prefix_", "prefix_" )]
        [TestCase( @"format -s [TmpFile.resw] --delete --start-with -p prefix_", "prefix_" )]
        [TestCase( @"format -s [TmpFile.Yaml] --delete --start-with -p prefix_", "prefix_" )]
        public void RemovePrefixToTheSameFile( string commandLine, string prefix )
        {
            // prepare tmp file with prefix
            var argsOrigin = Run( $"format -s [TmpFile] --start-with -p {prefix}" );
            var filePath = argsOrigin.TemporaryFiles[0];

            // remove prefix
            Run( commandLine, argsOrigin );

            var res = new ResourceFile( filePath );
            ValidateElements( res );
        }
    }
}