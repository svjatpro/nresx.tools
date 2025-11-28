using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Update
{
    [TestFixture]
    public class UpdateElementInSingleFileTests : TestBase
    {
        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateSingleElement( string commandLine )
        {
            var res1 = GetExampleResourceFile();
            var elementToUpdate = res1.Elements.Skip( 1 ).First();

            commandLine
                .PrepareArgs( () => new CommandLineParameters { UniqueKeys = { elementToUpdate.Key } } )
                .WithParams( args => new
                {
                    key = elementToUpdate.Key, 
                    value = args.UniqueKeys[1], 
                    comment = args.UniqueKeys.Count > 2 ? args.UniqueKeys[2] : null
                } )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( ( args, param ) =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    res.Elements.First( el => el.Key == param.key ).Value.Should().NotBe( param.value );
                } )
                .ValidateRun( ( args, param ) =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    var element = res.Elements.First( el => el.Key == param.key );
                    element.Value.Should().Be( param.value );
                    if ( res.ElementHasComment && param.comment != null ) 
                        element.Comment.Should().Be( param.comment );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"'{elementToUpdate.Key}' element have been updated in '{args.TemporaryFiles[0]}'" );
                } );
        }

        [TestCase( @"update [Output]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --new-file" )] // ignored for update
        [TestCase( @"update [Output]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        [TestCase( @"update [Output]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --new-file" )] // ignored for update
        public void UpdateNonExistingFile( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            var file = GetOutputPath( args.UniqueKeys[0] );
            new FileInfo( file ).Exists.Should().BeFalse();
            
            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( FilesNotFoundErrorMessage, file ) );
        }
        
        [TestCase( @"update [UniqueKey]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [UniqueKey]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [UniqueKey]\[UniqueKey].resx -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --new-file --recursive" )] // ignored for update
        [TestCase( @"update [UniqueKey]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey]" )]
        [TestCase( @"update [UniqueKey]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey]" )]
        [TestCase( @"update [UniqueKey]\[UniqueKey].resx --key [UniqueKey] --value [UniqueKey] --comment [UniqueKey] --new-file --recursive" )] // ignored for update
        public void UpdateNonExistingDir( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );

            var file = $"{args.UniqueKeys[0]}\\{args.UniqueKeys[1]}.resx";
            new FileInfo( file ).Exists.Should().BeFalse();

            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( DirectoryNotFoundErrorMessage, file ) );
        }

        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey]" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey]" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey]" )]
        public void UpdateNonExistingElement( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine );
            
            var res = new ResourceFile( args.TemporaryFiles[0] );
            var element = res.Elements.FirstOrDefault( el => el.Key == args.UniqueKeys[0] );
            element?.Should().BeNull();

            args.ConsoleOutput.Should().BeEquivalentTo( $"fatal: '{args.UniqueKeys[0]}' element not found" );
        }
        

        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey] --new-element" )]
        [TestCase( @"update [TmpFile] -k [UniqueKey] -v [UniqueKey] -c [UniqueKey] --new-element" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey] --new-element" )]
        [TestCase( @"update [TmpFile] --key [UniqueKey] -v [UniqueKey] --comment [UniqueKey] --new-element" )]
        public void UpdateNonExistingElementShouldCreateOne( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateDryRun( args =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    res.Elements.FirstOrDefault( el => el.Key == args.UniqueKeys[0] ).Should().BeNull();
                } )
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    var element = res.Elements.First( el => el.Key == args.UniqueKeys[0] );
                    element.Key.Should().Be( args.UniqueKeys[0] );
                    element.Value.Should().Be( args.UniqueKeys[1] );
                    if ( res.ElementHasComment && args.UniqueKeys.Count > 2 )
                        element.Comment.Should().Be( args.UniqueKeys[2] );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"'{args.UniqueKeys[0]}: {args.UniqueKeys[1]}' element have been added in '{args.TemporaryFiles[0]}'" );
                } );
        }
    }
}