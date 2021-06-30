using FluentAssertions;
using nresx.Core.Tests;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Validate
{
    [TestFixture]
    public class ValidateMultipleResourcesTests : TestBase
    {
        [TestCase( @"validate [TmpFile] [TmpFile]" )]
        [TestCase( @"validate -s [TmpFile] [TmpFile]" )]
        [TestCase( @"validate --source [TmpFile] [TmpFile]" )]
        public void ValidateResourceList( string commandLine )
        {
            var res = GetExampleResourceFile();
            TestHelper.PrepareCommandLine( commandLine, out var preArgs );

            TestHelper.ReplaceKey( preArgs.TemporaryFiles[0], res.Elements[1].Value, "" );
            TestHelper.ReplaceKey( preArgs.TemporaryFiles[0], res.Elements[2].Key, res.Elements[1].Key );
            TestHelper.ReplaceKey( preArgs.TemporaryFiles[1], res.Elements[1].Key, "" );

            var args = TestHelper.RunCommandLine( commandLine, preArgs );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"EmptyValue: {res.Elements[1].Key};",
                $"Duplicate: {res.Elements[1].Key};",
                $"EmptyKey: (value: {res.Elements[1].Value});" );
        }

        [TestCase( @"validate [Output]\[UniqueKey]*" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]*" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]*.*" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]*" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]*.*" )]
        public void ValidateBySpec( string commandLine )
        {
            var res = GetExampleResourceFile();
            var files = PrepareTemporaryFiles( 2, 1, out var key1 );

            TestHelper.ReplaceKey( files[0], res.Elements[1].Value, "" );
            TestHelper.ReplaceKey( files[0], res.Elements[2].Key, res.Elements[1].Key );
            TestHelper.ReplaceKey( files[1], res.Elements[1].Key, "" );
            TestHelper.ReplaceKey( files[2], res.Elements[1].Value, "" ); //

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters {UniqueKeys = { key1 }} );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"EmptyValue: {res.Elements[1].Key};",
                $"Duplicate: {res.Elements[1].Key};",
                $"EmptyKey: (value: {res.Elements[1].Value});" );
        }

        [TestCase( @"validate [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]*.* -r" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]* --recursive" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]*.* --recursive" )]
        public void ValidateBySpecRecursive( string commandLine )
        {
            var res = GetExampleResourceFile();
            var files = PrepareTemporaryFiles( 2, 1, out var key1 );

            TestHelper.ReplaceKey( files[0], res.Elements[1].Value, "" );
            TestHelper.ReplaceKey( files[0], res.Elements[2].Key, res.Elements[1].Key );
            TestHelper.ReplaceKey( files[1], res.Elements[1].Key, "" );
            TestHelper.ReplaceKey( files[2], res.Elements[1].Value, "" ); //

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { UniqueKeys = { key1 } } );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"EmptyValue: {res.Elements[1].Key};",
                $"Duplicate: {res.Elements[1].Key};",
                $"EmptyKey: (value: {res.Elements[1].Value});",
                $"EmptyValue: {res.Elements[1].Key};" );
        }
    }
}