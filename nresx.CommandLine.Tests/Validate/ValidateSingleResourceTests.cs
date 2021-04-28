using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Validate
{
    [TestFixture]
    public class ValidateSingleResourceTests : TestBase
    {
        [TestCase( @"validate [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile]" )]
        public void ValidateDuplicatedElements( string commandLine )
        {
            PrepareCommandLine( commandLine, out var preArgs );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            res.Elements.Add( res.Elements[1].Key, UniqueKey() ); // add duplicated element
            res.Save( file );

            var args = Run( commandLine, new CommandLineParameters{ TemporaryFiles = { file }} );
            
            args.ConsoleOutput[0].Should().Be( $"Resource file name: \"{res.Name}\", (\"{res.AbsolutePath})\"" );
            args.ConsoleOutput[1].Should().Be( $"resource format type: {res.ResourceFormat}" );
            args.ConsoleOutput[2].Should().Be( $"text elements: {res.Elements.Count()}" );
        }
    }
}