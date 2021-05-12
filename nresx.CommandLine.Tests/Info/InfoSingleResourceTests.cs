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
            var args = Run( commandLine );
            var res = new ResourceFile( args.TemporaryFiles[0] );

            args.ConsoleOutput[0].Should().Be( $"Resource file name: \"{res.Name}\", (\"{res.AbsolutePath})\"" );
            args.ConsoleOutput[1].Should().Be( $"resource format type: {res.ResourceFormat}" );
            args.ConsoleOutput[2].Should().Be( $"text elements: {res.Elements.Count()}" );
        }
    }
}