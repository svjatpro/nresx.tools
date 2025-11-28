using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Validate
{
    [TestFixture]
    public class ValidateGroupedResourcesTests : TestBase
    {
        [TestCase( @"validate [Output]\[UniqueKey]*" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]*" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]*" )]
        public void ValidateMissedElementSingleFolder( string commandLine )
        {
            var files = PrepareGroupedFiles( new []{"en", "fr"}, out var key1 );
            
            var resEn = new ResourceFile( files[0] );
            resEn.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resEn.Save( files[0] );

            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0].Value = TestData.UniqueKey();
            resFr.Elements[1].Value = TestData.UniqueKey();
            resFr.Elements[2].Value = TestData.UniqueKey();
            resFr.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resFr.Save( files[1] );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters{ UniqueKeys = { key1 } } );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                $"MissedElement: {resFr.Elements.Last().Key};",
                $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                $"MissedElement: {resEn.Elements.Last().Key};" );
        }

        [TestCase( @"validate [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]* --recursive" )]
        public void ValidateMissedElementSingleFolderRecursive( string commandLine )
        {
            var files = PrepareGroupedFiles( new[] { "en", "fr" }, out var key1, dir: TestData.UniqueKey() );

            var resEn = new ResourceFile( files[0] );
            resEn.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resEn.Save( files[0] );

            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0].Value = TestData.UniqueKey();
            resFr.Elements[1].Value = TestData.UniqueKey();
            resFr.Elements[2].Value = TestData.UniqueKey();
            resFr.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resFr.Save( files[1] );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { UniqueKeys = { key1 } } );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                $"MissedElement: {resFr.Elements.Last().Key};",
                $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                $"MissedElement: {resEn.Elements.Last().Key};" );
        }

        [TestCase( @"validate [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]* --recursive" )]
        public void ValidateMissedElementLocaleFolders( string commandLine )
        {
            var files = PrepareGroupedFiles( new[] { "en", "fr" }, out var key1, dir: TestData.UniqueKey(), dirLocales: true );

            var resEn = new ResourceFile( files[0] );
            resEn.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resEn.Save( files[0] );

            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0].Value = TestData.UniqueKey();
            resFr.Elements[1].Value = TestData.UniqueKey();
            resFr.Elements[2].Value = TestData.UniqueKey();
            resFr.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resFr.Save( files[1] );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { UniqueKeys = { key1 } } );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                $"MissedElement: {resFr.Elements.Last().Key};",
                $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                $"MissedElement: {resEn.Elements.Last().Key};" );
        }


        [TestCase( @"validate [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]* --recursive" )]
        public void ValidateNotTranslatedSingleFolderRecursive( string commandLine )
        {
            var files = PrepareGroupedFiles( new[] { "en", "fr" }, out var key1, dir: TestData.UniqueKey() );

            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0].Value = TestData.UniqueKey();
            resFr.Elements[2].Value = TestData.UniqueKey();
            resFr.Save( files[1] );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { UniqueKeys = { key1 } } );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                $"NotTranslated: {resFr.Elements[1].Key};",
                $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                $"NotTranslated: {resFr.Elements[1].Key};" );
        }

        [TestCase( @"validate [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate -s [Output]\[UniqueKey]* -r" )]
        [TestCase( @"validate --source [Output]\[UniqueKey]* --recursive" )]
        public void ValidateNotTranslatedLocaleFolders( string commandLine )
        {
            var files = PrepareGroupedFiles( new[] { "en", "fr" }, out var key1, dir: TestData.UniqueKey(), dirLocales: true );

            var resFr = new ResourceFile( files[1] );
            resFr.Elements[0].Value = TestData.UniqueKey();
            resFr.Elements[2].Value = TestData.UniqueKey();
            resFr.Save( files[1] );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { UniqueKeys = { key1 } } );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"Resource file: \"{new FileInfo( files[0] ).FullName}\"",
                $"NotTranslated: {resFr.Elements[1].Key};",
                $"Resource file: \"{new FileInfo( files[1] ).FullName}\"",
                $"NotTranslated: {resFr.Elements[1].Key};" );
        }
    }
}