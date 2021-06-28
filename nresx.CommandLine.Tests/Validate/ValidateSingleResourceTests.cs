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
        public void ValidateEmptyValueElements( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            res.Elements[1].Value = string.Empty;
            res.Save( file );

            var args = Run( commandLine, new CommandLineParameters{ TemporaryFiles = { file }} );
            
            args.ConsoleOutput[0].Should().Be( $"EmptyValue: {res.Elements[1].Key}; " );
        }
    }
}