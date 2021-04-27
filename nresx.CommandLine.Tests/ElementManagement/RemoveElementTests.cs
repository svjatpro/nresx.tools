using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.ElementManagement
{
    [TestFixture]
    public class RemoveElementTests : TestBase
    {
        [TestCase( @"remove [TmpFile] -k [UniqueKey]" )]
        [TestCase( @"remove [TmpFile] --key [UniqueKey]" )]
        public void RemoveSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToDelete = res1.Elements.First();
            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { elementToDelete.Key }} );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            res.Elements.Should().NotContain( el => el.Key == elementToDelete.Key );
        }
    }
}