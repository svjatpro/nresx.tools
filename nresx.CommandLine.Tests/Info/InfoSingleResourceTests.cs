using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Info
{
    [TestFixture]
    public class InfoSingleResourceTests : TestBase
    {
        [TestCase( @"[TmpFile]" )]
        [TestCase( @"info -s [TmpFile.yaml]" )]
        [TestCase( @"info --source [TmpFile]" )]
        public void GetSingleFileInfo( string commandLine )
        {
            commandLine
                .ValidateRun( _ => { } )
                .ValidateStdout( args =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"Resource file name: \"{res.FileName}\", (\"{res.AbsolutePath})\"",
                        $"resource format type: {res.FileFormat}",
                        $"text elements: {res.Elements.Count()}" );
                } );
        }
    }
}