using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.ElementManagement
{
    [TestFixture]
    public class AddElementTests : TestBase
    {
        [TestCase( @"add [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"add [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"add [TmpFile] --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"add [TmpFile] --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        public void AddSingleElement( string commandLine )
        {
            var args = Run( commandLine );
            
            var file = args.TemporaryFiles[0];
            var key = args.UniqueKeys[0];
            var value = args.UniqueKeys[1];
            var comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : string.Empty;

            var res = new ResourceFile( file );
            res.Elements.Should().Contain( el => 
                el.Key == key && el.Value == value && el.Comment == comment );
        }
    }
}