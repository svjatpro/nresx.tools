using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Update
{
    [TestFixture]
    public class UpdateElementTests : TestBase
    {
        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();
            var args = Run( commandLine, new CommandLineParameters{UniqueKeys = { elementToUpdate.Key }} );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            var element = res.Elements.First( el => el.Key == elementToUpdate.Key );
            element.Value.Should().Be( args.UniqueKeys[0] );
            if( args.UniqueKeys.Count > 1 ) element.Comment.Should().Be( args.UniqueKeys[1] );
        }
    }
}