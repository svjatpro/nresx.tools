using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Format
{
    [TestFixture]
    public class FormatMultipleResourceTests : FormatBasicTests
    {
        /// <summary>
        /// pattern: {0} - element value, {1} - pattern
        /// </summary>
        [TestCase( @"format [TmpFile] [TmpFile] --start-with -p [UniqueKey]", "{1}{0}" )]
        [TestCase( @"format -s [TmpFile] [TmpFile] --start-with -p [UniqueKey]", "{1}{0}" )]
        [TestCase( @"format --source [TmpFile] [TmpFile] --start-with --pattern [UniqueKey]", "{1}{0}" )]
        public void FormatTwoFiles( string commandLine, string template )
        {
            var pattern = TestData.UniqueKey();
            var file1 = TestHelper.CopyTemporaryFile();
            var file2 = TestHelper.CopyTemporaryFile();
            var elements = GetExampleResourceFile().Elements.ToDictionary( el => el.Key, el => el.Value );

            // format resource file
            commandLine
                .PrepareArgs( () => new CommandLineParameters { TemporaryFiles = { file1, file2 }, UniqueKeys = { pattern } } )
                .ValidateDryRun( args =>
                {
                    ValidateElements( new ResourceFile( args.TemporaryFiles[0] ) );
                    ValidateElements( new ResourceFile( args.TemporaryFiles[1] ) );
                } )
                .ValidateRun( args =>
                {
                    new ResourceFile( args.TemporaryFiles[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( args.TemporaryFiles[1] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( 
                        string.Format( SuccessLineTemplate, elements.Count, args.TemporaryFiles[0] ),
                        string.Format( SuccessLineTemplate, elements.Count, args.TemporaryFiles[1] ) );
                } );

            // (delete) revert changes
            commandLine = $"{commandLine} --delete";
            commandLine
                .PrepareArgs( () => new CommandLineParameters { TemporaryFiles = { file1, file2 }, UniqueKeys = { pattern } } )
                .ValidateDryRun( args =>
                {
                    new ResourceFile( args.TemporaryFiles[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( args.TemporaryFiles[1] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                } )
                .ValidateRun( args =>
                {
                    ValidateElements( new ResourceFile( args.TemporaryFiles[0] ) );
                    ValidateElements( new ResourceFile( args.TemporaryFiles[1] ) );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        string.Format( SuccessLineTemplate, elements.Count, args.TemporaryFiles[0] ),
                        string.Format( SuccessLineTemplate, elements.Count, args.TemporaryFiles[1] ) );
                } );
        }

        [TestCase( @"format [Output]\[UniqueKey]* --start-with -p [UniqueKey]", "{1}{0}" )]
        [TestCase( @"format -s [Output]\[UniqueKey]* --start-with -p [UniqueKey]", "{1}{0}" )]
        [TestCase( @"format -s [Output]\[UniqueKey]*.* --start-with -p [UniqueKey]", "{1}{0}" )]
        [TestCase( @"format --source [Output]\[UniqueKey]* --start-with --pattern [UniqueKey]", "{1}{0}" )]
        [TestCase( @"format --source [Output]\[UniqueKey]*.* --start-with --pattern [UniqueKey]", "{1}{0}" )]
        public void FormatBySpec( string commandLine, string template )
        {
            var pattern = TestData.UniqueKey();
            var files = PrepareTemporaryFiles( 2, 1, out var key1 );
            var elements = GetExampleResourceFile().Elements.ToDictionary( el => el.Key, el => el.Value );

            // format resource file
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1, pattern } } )
                .ValidateDryRun( args =>
                {
                    ValidateElements( new ResourceFile( files[0] ) );
                    ValidateElements( new ResourceFile( files[1] ) );
                    ValidateElements( new ResourceFile( files[2] ) );
                } )
                .ValidateRun( args =>
                {
                    new ResourceFile( files[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( files[1] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    ValidateElements( new ResourceFile( files[2] ) );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        string.Format( SuccessLineTemplate, elements.Count, files[0] ),
                        string.Format( SuccessLineTemplate, elements.Count, files[1] ) );
                } );

            // (delete) revert changes
            commandLine = $"{commandLine} --delete";
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1, pattern } } )
                .ValidateDryRun( args =>
                {
                    new ResourceFile( files[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( files[1] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    ValidateElements( new ResourceFile( files[2] ) );
                } )
                .ValidateRun( args =>
                {
                    ValidateElements( new ResourceFile( files[0] ) );
                    ValidateElements( new ResourceFile( files[1] ) );
                    ValidateElements( new ResourceFile( files[2] ) );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        string.Format( SuccessLineTemplate, elements.Count, files[0] ),
                        string.Format( SuccessLineTemplate, elements.Count, files[1] ) );
                } );
        }

        [TestCase( @"format [Output]\[UniqueKey]* --start-with -p [UniqueKey] -r", "{1}{0}" )]
        [TestCase( @"format -s [Output]\[UniqueKey]* --start-with -p [UniqueKey] -r", "{1}{0}" )]
        [TestCase( @"format -s [Output]\[UniqueKey]*.* --start-with -p [UniqueKey] -r", "{1}{0}" )]
        [TestCase( @"format --source [Output]\[UniqueKey]* --start-with --pattern [UniqueKey] -r", "{1}{0}" )]
        [TestCase( @"format --source [Output]\[UniqueKey]*.* --start-with --pattern [UniqueKey] -r", "{1}{0}" )]
        public void FormatBySpecRecursive( string commandLine, string template )
        {
            var pattern = TestData.UniqueKey();
            var files = PrepareTemporaryFiles( 2, 1, out var key1 );
            var elements = GetExampleResourceFile().Elements.ToDictionary( el => el.Key, el => el.Value );

            // format resource file
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1, pattern } } )
                .ValidateDryRun( args =>
                {
                    ValidateElements( new ResourceFile( files[0] ) );
                    ValidateElements( new ResourceFile( files[1] ) );
                } )
                .ValidateRun( args =>
                {
                    new ResourceFile( files[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( files[1] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( files[2] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        string.Format( SuccessLineTemplate, elements.Count, files[0].GetShortPath() ),
                        string.Format( SuccessLineTemplate, elements.Count, files[1].GetShortPath() ),
                        string.Format( SuccessLineTemplate, elements.Count, files[2].GetShortPath() ) );
                } );

            // (delete) revert changes
            commandLine = $"{commandLine} --delete";
            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { key1, pattern } } )
                .ValidateDryRun( args =>
                {
                    new ResourceFile( files[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( files[1] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                    new ResourceFile( files[2] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                } )
                .ValidateRun( args =>
                {
                    ValidateElements( new ResourceFile( files[0] ) );
                    ValidateElements( new ResourceFile( files[1] ) );
                    ValidateElements( new ResourceFile( files[2] ) );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        string.Format( SuccessLineTemplate, elements.Count, files[0].GetShortPath() ),
                        string.Format( SuccessLineTemplate, elements.Count, files[1].GetShortPath() ),
                        string.Format( SuccessLineTemplate, elements.Count, files[2].GetShortPath() ) );
                } );
        }
    }
}