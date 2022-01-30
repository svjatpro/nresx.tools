using System.IO;
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
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .BeforeRun( args =>
                {
                    TestHelper.ReplaceKey( args.TemporaryFiles[0], res.Elements[1].Value, "" );
                    TestHelper.ReplaceKey( args.TemporaryFiles[0], res.Elements[2].Key, res.Elements[1].Key );
                    TestHelper.ReplaceKey( args.TemporaryFiles[1], res.Elements[1].Key, "" );
                } )
                .ValidateRun( () => {} )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"Resource file: \"{new FileInfo( args.TemporaryFiles[0] ).FullName}\"",
                        $"EmptyValue: {res.Elements[1].Key};",
                        $"Duplicate: {res.Elements[1].Key};",
                        $"Resource file: \"{new FileInfo( args.TemporaryFiles[1] ).FullName}\"",
                        $"EmptyKey: (value: {res.Elements[1].Value});" );
                } );
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
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .PrepareArgs( () => new CommandLineParameters{ UniqueKeys = { key1 } } )
                .BeforeRun( args =>
                {
                    TestHelper.ReplaceKey( files[0], res.Elements[1].Value, "" );
                    TestHelper.ReplaceKey( files[0], res.Elements[2].Key, res.Elements[1].Key );
                    TestHelper.ReplaceKey( files[1], res.Elements[1].Key, "" );
                    TestHelper.ReplaceKey( files[2], res.Elements[1].Value, "" ); //
                } )
                .ValidateRun( () => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                        $"EmptyValue: {res.Elements[1].Key};",
                        $"Duplicate: {res.Elements[1].Key};",
                        $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                        $"EmptyKey: (value: {res.Elements[1].Value});" );
                } );
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
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1 } } )
                .BeforeRun( args =>
                {
                    TestHelper.ReplaceKey( files[0], res.Elements[1].Value, "" );
                    TestHelper.ReplaceKey( files[0], res.Elements[2].Key, res.Elements[1].Key );
                    TestHelper.ReplaceKey( files[1], res.Elements[1].Key, "" );
                    TestHelper.ReplaceKey( files[2], res.Elements[1].Value, "" ); //
                } )
                .ValidateRun( () => { } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                        $"EmptyValue: {res.Elements[1].Key};",
                        $"Duplicate: {res.Elements[1].Key};",
                        $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                        $"EmptyKey: (value: {res.Elements[1].Value});",
                        $"Resource file: \"{new FileInfo( files[2] ).FullName}\"",
                        $"EmptyValue: {res.Elements[1].Key};" );
                } );
        }
    }
}