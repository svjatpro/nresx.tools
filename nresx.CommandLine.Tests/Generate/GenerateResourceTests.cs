using System.Linq;
using FluentAssertions;
using nresx.CommandLine.Tests.Format;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Generate
{
    [TestFixture]
    public class GenerateResourceTests : FormatBasicTests
    {
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