using System;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Core;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Validate
{
    // `validate --format json` (RSX-232): one JSON document on stdout, fatal errors stay
    // on stderr, exit codes identical to text mode. Assertions parse the stdout-only
    // capture (StdOutput) - ConsoleOutput interleaves stderr and must not be used here.
    [TestFixture]
    public class ValidateJsonOutputTests : TestBase
    {
        private static JsonElement ParseStdout( CommandLineParameters args )
        {
            var raw = string.Join( Environment.NewLine, args.StdOutput );
            raw.Should().NotBeNullOrWhiteSpace( "json mode must emit a document to stdout" );
            return JsonDocument.Parse( raw ).RootElement;
        }

        [TestCase( @"validate [TmpFile] --format json" )]
        [TestCase( @"validate [TmpFile] -f json" )]
        [TestCase( @"validate [TmpFile] -f JSON" )] // case-insensitive
        public void CleanFileGivesEmptyIssuesAndExitZero( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitSuccess );
            var doc = ParseStdout( args );
            doc.GetProperty( "tool" ).GetString().Should().Be( "nresx" );
            doc.GetProperty( "version" ).GetString().Should().Be( ResourceManager.GetVersion().TrimStart( 'v' ) );
            doc.GetProperty( "issues" ).GetArrayLength().Should().Be( 0 );
            doc.GetProperty( "summary" ).GetProperty( "issues" ).GetInt32().Should().Be( 0 );
            doc.GetProperty( "summary" ).GetProperty( "errors" ).GetInt32().Should().Be( 0 );
            doc.GetProperty( "summary" ).GetProperty( "warnings" ).GetInt32().Should().Be( 0 );
        }

        [TestCase( @"validate [TmpFile] --format json" )]
        public void DuplicateKeyGivesErrorIssueAndExitNonZero( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true, RequireMangleable = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            TestHelper.ReplaceKey( file, res.Elements[2].Key, res.Elements[1].Key );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            args.ExitCode.Should().Be( ExitFailure );
            var doc = ParseStdout( args );
            var issues = doc.GetProperty( "issues" ).EnumerateArray().ToList();
            issues.Should().Contain( i =>
                i.GetProperty( "rule" ).GetString() == "Duplicate" &&
                i.GetProperty( "severity" ).GetString() == "error" &&
                i.GetProperty( "key" ).GetString() == res.Elements[1].Key );
            doc.GetProperty( "summary" ).GetProperty( "errors" ).GetInt32().Should().BeGreaterThan( 0 );
        }

        [TestCase( @"validate [Output]/[UniqueKey]* --format json" )]
        public void TranslationGroupReportsMissedElements( string commandLine )
        {
            var files = PrepareGroupedFiles( new[] { "en", "fr" }, out var key1 );

            var resEn = new ResourceFile( files[0] );
            resEn.Elements.Add( TestData.UniqueKey(), TestData.UniqueKey() );
            resEn.Save( files[0] );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { UniqueKeys = { key1 } } );

            args.ExitCode.Should().Be( ExitSuccess ); // MissedElement is a warning
            var doc = ParseStdout( args );
            var issues = doc.GetProperty( "issues" ).EnumerateArray().ToList();
            issues.Should().Contain( i =>
                i.GetProperty( "rule" ).GetString() == "MissedElement" &&
                i.GetProperty( "severity" ).GetString() == "warning" &&
                i.GetProperty( "key" ).GetString() == resEn.Elements.Last().Key );
            doc.GetProperty( "summary" ).GetProperty( "warnings" ).GetInt32().Should().BeGreaterThan( 0 );
        }

        [TestCase( @"validate [TmpFile] --format json", false )]
        [TestCase( @"validate [TmpFile] --format json --warnings-as-errors", true )]
        public void WarningsAsErrorsFlipsExitCode( string commandLine, bool expectFailure )
        {
            TestHelper.PrepareCommandLine( commandLine, out var preArgs, options: new CommandRunOptions { SkipFilesWithoutKey = true } );
            var file = preArgs.TemporaryFiles[0];
            var res = new ResourceFile( file );
            res.Elements[1].Value = string.Empty; // EmptyValue = warning
            res.Save( file );

            var args = TestHelper.RunCommandLine( commandLine, new CommandLineParameters { TemporaryFiles = { file } } );

            if ( expectFailure )
                args.ExitCode.Should().NotBe( ExitSuccess );
            else
                args.ExitCode.Should().Be( ExitSuccess );

            var doc = ParseStdout( args );
            doc.GetProperty( "summary" ).GetProperty( "warnings" ).GetInt32().Should().BeGreaterThan( 0 );
        }

        [TestCase( @"validate [TmpFile] --format xml" )]
        [TestCase( @"validate [TmpFile] -f yaml" )]
        public void UnknownOutputFormatIsUsageError( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitUsageError );
            args.StdOutput.Should().BeEmpty( "usage errors must not emit a json document" );
            var formatValue = commandLine.Split( ' ' ).Last();
            args.ConsoleOutput.Should().Contain( string.Format( UnknownOutputFormatErrorMessage, formatValue ) );
        }

        [TestCase( @"validate [Output]/[UniqueKey]*.resx --format json" )]
        public void WrongFileSpecKeepsStdoutParseable( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            args.ExitCode.Should().Be( ExitNotFound );
            args.ConsoleOutput.Should().ContainSingle( line => line.StartsWith( "fatal: path mask" ) );
            var doc = ParseStdout( args ); // stderr noise must not contaminate the document
            doc.GetProperty( "issues" ).GetArrayLength().Should().Be( 0 );
        }
    }
}
