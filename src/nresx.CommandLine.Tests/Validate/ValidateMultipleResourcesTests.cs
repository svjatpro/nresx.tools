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
                .WithOptions( opt => { opt.SkipFilesWithoutKey = true; opt.RequireMangleable = true; } )
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
                        $"{new FileInfo( args.TemporaryFiles[0] ).FullName}: warning: EmptyValue: {res.Elements[1].Key}",
                        $"{new FileInfo( args.TemporaryFiles[0] ).FullName}: error: Duplicate: {res.Elements[1].Key}",
                        $"{new FileInfo( args.TemporaryFiles[1] ).FullName}: error: EmptyKey: (value: {res.Elements[1].Value})",
                        "Found 3 issues (2 errors, 1 warning)" );
                } );
        }

        [TestCase( @"validate [Output]/[UniqueKey]*" )]
        [TestCase( @"validate -s [Output]/[UniqueKey]*" )]
        [TestCase( @"validate -s [Output]/[UniqueKey]*.*" )]
        [TestCase( @"validate --source [Output]/[UniqueKey]*" )]
        [TestCase( @"validate --source [Output]/[UniqueKey]*.*" )]
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
                        $"{new FileInfo( files[0] ).FullName}: warning: EmptyValue: {res.Elements[1].Key}",
                        $"{new FileInfo( files[0] ).FullName}: error: Duplicate: {res.Elements[1].Key}",
                        $"{new FileInfo( files[1] ).FullName}: error: EmptyKey: (value: {res.Elements[1].Value})",
                        "Found 3 issues (2 errors, 1 warning)" );
                } );
        }

        [TestCase( @"validate [Output]/[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]/[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]/[UniqueKey]*.* -r" )]
        [TestCase( @"validate --source [Output]/[UniqueKey]* --recursive" )]
        [TestCase( @"validate --source [Output]/[UniqueKey]*.* --recursive" )]
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
                        $"{new FileInfo( files[0] ).FullName}: warning: EmptyValue: {res.Elements[1].Key}",
                        $"{new FileInfo( files[0] ).FullName}: error: Duplicate: {res.Elements[1].Key}",
                        $"{new FileInfo( files[1] ).FullName}: error: EmptyKey: (value: {res.Elements[1].Value})",
                        $"{new FileInfo( files[2] ).FullName}: warning: EmptyValue: {res.Elements[1].Key}",
                        "Found 4 issues (2 errors, 2 warnings)" );
                } );
        }
    }
}