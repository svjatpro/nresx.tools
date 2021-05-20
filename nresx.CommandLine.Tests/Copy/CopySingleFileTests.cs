using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Copy
{
    [TestFixture]
    public class CopySingleFileTests : TestBase
    {
        [TestCase( @"copy [TmpFile] [NewFile]" )]
        [TestCase( @"copy [TmpFile] -d [NewFile]" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile]" )]
        public void CopyToNonExistingFile( string commandLine )
        {
            var args = Run( commandLine );
            
            var res = new ResourceFile( args.NewFiles[0] );
            ValidateElements( res );
        }

        [TestCase( @"copy [TmpFile] [NewFile] --dry-run" )]
        [TestCase( @"copy [TmpFile] -d [NewFile] --dry-run" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile] --dry-run" )]
        public void CopyToNonExistingFileDryRun( string commandLine )
        {
            var args = Run( commandLine );

            new FileInfo( args.NewFiles[0] ).Exists.Should().BeFalse();

            var res = new ResourceFile( args.TemporaryFiles[0] );
            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"'{res.Elements[0].Key}' element have been copied to '{args.NewFiles[0]}' file",
                $"'{res.Elements[1].Key}' element have been copied to '{args.NewFiles[0]}' file",
                $"'{res.Elements[2].Key}' element have been copied to '{args.NewFiles[0]}' file" );
        }
        
    }
}