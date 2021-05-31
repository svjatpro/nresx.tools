using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Format
{
    [TestFixture]
    public class FormatSingleResourceTests : FormatBasicTests
    {
        /// <summary>
        /// pattern: {0} - element value, {1} - pattern
        /// </summary>
        
        // start-with
        [TestCase( @"format [TmpFile] --start-with -p [UniqueKey]", "en", "{1}{0}" )]
        [TestCase( @"format -s [TmpFile] --start-with -p [UniqueKey]", "en", "{1}{0}" )]
        [TestCase( @"format -s [TmpFile.resw] --start-with -p [UniqueKey]", "en", "{1}{0}" )]
        [TestCase( @"format -s [TmpFile.Yaml] --start-with -p [UniqueKey]", "en", "{1}{0}" )]
        [TestCase( @"format --source [TmpFile] --start-with --pattern [UniqueKey]", "en", "{1}{0}" )]

        // end-with
        [TestCase( @"format [TmpFile] --end-with -p [UniqueKey]", "en", "{0}{1}" )]
        [TestCase( @"format -s [TmpFile] --end-with -p [UniqueKey]", "en", "{0}{1}" )]
        [TestCase( @"format -s [TmpFile.resw] --end-with -p [UniqueKey]", "en", "{0}{1}" )]
        [TestCase( @"format -s [TmpFile.Yaml] --end-with -p [UniqueKey]", "en", "{0}{1}" )]
        [TestCase( @"format --source [TmpFile] --end-with --pattern [UniqueKey]", "en", "{0}{1}" )]

        // language-code
        [TestCase( @"format [TmpFile] --start-with --language-code", "en", "en_{0}" )]
        [TestCase( @"format -s [TmpFile] --start-with --language-code", "en-US", "en_{0}" )]
        [TestCase( @"format --source [TmpFile] --start-with --language-code", "fr-CA", "fr_{0}" )]
        
        [TestCase( @"format [TmpFile] --end-with --language-code", "en", "{0}_en" )]
        [TestCase( @"format -s [TmpFile] --end-with --language-code", "fr-CA", "{0}_fr" )]
        [TestCase( @"format --source [TmpFile] --end-with --language-code", "uk", "{0}_uk" )]

        // culture-code
        [TestCase( @"format [TmpFile] --start-with --culture-code", "en", "en_{0}" )]
        [TestCase( @"format -s [TmpFile] --start-with --culture-code", "en-US", "en-US_{0}" )]
        [TestCase( @"format --source [TmpFile] --start-with --culture-code", "fr-CA", "fr-CA_{0}" )]

        [TestCase( @"format [TmpFile] --end-with --culture-code", "en", "{0}_en" )]
        [TestCase( @"format -s [TmpFile] --end-with --culture-code", "fr-CA", "{0}_fr-CA" )]
        [TestCase( @"format --source [TmpFile] --end-with --culture-code", "uk", "{0}_uk" )]
        public void FormatSingleFile( string commandLine, string code, string template )
        {
            var pattern = TestData.UniqueKey();
            var file = TestHelper.CopyTemporaryFile( destDir: code );
            var elements = GetExampleResourceFile().Elements.ToDictionary( el => el.Key, el => el.Value );

            // format resource file
            commandLine
                .PrepareArgs( () => new CommandLineParameters{ TemporaryFiles = { file }, UniqueKeys = { pattern } } )
                .ValidateDryRun( args => { ValidateElements( new ResourceFile( args.TemporaryFiles[0] ) ); } )
                .ValidateRun( args =>
                {
                    new ResourceFile( args.TemporaryFiles[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                } )
                .ValidateStdout( args => new[] {string.Format( SuccessLineTemplate, GetExampleResourceFile().Elements.Count(), args.TemporaryFiles[0] )} );

            // (delete) revert changes
            commandLine = $"{commandLine} --delete";
            commandLine
                .PrepareArgs( () => new CommandLineParameters { TemporaryFiles = { file }, UniqueKeys = { pattern } } )
                .ValidateDryRun( args =>
                {
                    new ResourceFile( args.TemporaryFiles[0] )
                        .Elements.Where( el => el.Value == string.Format( template, elements[el.Key], pattern ) )
                        .Should().HaveCount( elements.Count );
                } )
                .ValidateRun( args => { ValidateElements( new ResourceFile( args.TemporaryFiles[0] ) ); } )
                .ValidateStdout( args => new[] { string.Format( SuccessLineTemplate, GetExampleResourceFile().Elements.Count(), args.TemporaryFiles[0] ) } );
        }
    }
}