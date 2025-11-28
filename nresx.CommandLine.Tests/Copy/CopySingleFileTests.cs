using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Copy
{
    [TestFixture]
    public class CopySingleFileTests : TestBase
    {
        #region Private members

        private void PrepareFromToResources(
            out string resFromPath, out string resToPath,
            out ResourceElement updated, out ResourceElement newFrom, out ResourceElement newTo )
        {
            resFromPath = TestHelper.CopyTemporaryFile();
            resToPath = TestHelper.CopyTemporaryFile();

            var resFrom = new ResourceFile( resFromPath );
            // element, which has updated value in From file, will be skipped
            var keyUpdated = resFrom.Elements[1].Key;
            updated = resFrom.Elements[keyUpdated];
            updated.Value = UniqueKey();

            // element, which exists in From, and not exists in To file, will be added
            var keyNewFrom = UniqueKey();
            resFrom.Elements.Add( keyNewFrom, UniqueKey(), UniqueKey() );
            resFrom.Save( resFromPath );
            newFrom = resFrom.Elements[keyNewFrom];

            var resTo = new ResourceFile( resToPath );
            // element, which exists in To, and not exists in From file, will be skipped
            var keyNewTo = UniqueKey();
            resTo.Elements.Add( keyNewTo, UniqueKey(), UniqueKey() );
            resTo.Save( resToPath );
            newTo = resTo.Elements[keyNewTo];
        }

        #endregion

        // todo: add specific tests for .txt resources

        [TestCase( @"copy [TmpFile] [NewFile]" )]
        [TestCase( @"copy [TmpFile] -d [NewFile]" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile]" )]
        public void CopyToNonExistingFile( string commandLine )
        {
            var args = TestHelper.RunCommandLine( commandLine, options: new CommandRunOptions { SkipFilesWithoutKey = true } );

            new FileInfo( args.NewFiles[0] ).Exists.Should().BeFalse();
            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( FilesNotFoundErrorMessage, args.NewFiles[0].GetShortPath() ) );
        }
        
        [TestCase( @"copy [TmpFile] [NewFile] --new-file" )]
        [TestCase( @"copy [TmpFile] -d [NewFile] --new-file" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile] --new-file" )]
        public void CopyToNonExistingFileShouldCreateOne( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .WithParams( args => new { tmp = new ResourceFile( args.TemporaryFiles[0] ) } )
                .ValidateDryRun( args =>
                {
                    new FileInfo( args.NewFiles[0] ).Exists.Should().BeFalse();
                } )
                .ValidateRun( ( args, _ ) =>
                {
                    ValidateElements( new ResourceFile( args.NewFiles[0] ), new ResourceFile( args.TemporaryFiles[0] ) );
                } )
                .ValidateStdout( ( args, param ) =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{param.tmp.Elements[0].Key}' element have been copied to '{args.NewFiles[0]}' file",
                        $"'{param.tmp.Elements[1].Key}' element have been copied to '{args.NewFiles[0]}' file",
                        $"'{param.tmp.Elements[2].Key}' element have been copied to '{args.NewFiles[0]}' file" );
                } );
        }
        
        [TestCase( @"copy [TmpFile] [TmpFile] --skip" )]
        [TestCase( @"copy [TmpFile] [TmpFile]" )]
        [TestCase( @"copy [TmpFile] -d [TmpFile] --skip" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --skip" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile]" )]
        public void CopyToExistingFileShouldSkipDuplicates( string commandLine )
        {
            PrepareFromToResources( out var resFromPath, out var resToPath, out var updated, out var newFrom, out var newTo );
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .PrepareArgs( () => new CommandLineParameters{ TemporaryFiles = { resFromPath, resToPath }} )
                .ValidateDryRun( () =>
                {
                    var res = new ResourceFile( resToPath );
                    res.Elements[updated.Key].Value.Should().NotBe( updated.Value );
                    res.Elements.Should().NotContain( el => el.Key == newFrom.Key );
                    res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value );
                } )
                .ValidateRun( () =>
                {
                    var res = new ResourceFile( resToPath );
                    res.Elements[updated.Key].Value.Should().NotBe( updated.Value ); // skip updated
                    res.Elements.Should().ContainSingle( el => el.Key == newFrom.Key && el.Value == newFrom.Value ); // copy new element
                    res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value ); // new element in dest remains unchanged
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo( $"'{newFrom.Key}' element have been copied to '{resToPath}' file" );
                } );
        }

        [TestCase( @"copy [TmpFile] [TmpFile] --overwrite" )]
        [TestCase( @"copy [TmpFile] -d [TmpFile] --overwrite" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --overwrite" )]
        public void CopyToExistingFileShouldOverwriteDuplicates( string commandLine )
        {
            PrepareFromToResources( out var resFromPath, out var resToPath, out var updated, out var newFrom, out var newTo );
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .PrepareArgs( () => new CommandLineParameters { TemporaryFiles = { resFromPath, resToPath } } )
                .ValidateDryRun( () =>
                {
                    var res = new ResourceFile( resToPath );
                    res.Elements[updated.Key].Value.Should().NotBe( updated.Value );
                    res.Elements.Should().NotContain( el => el.Key == newFrom.Key );
                    res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value );
                } )
                .ValidateRun( () =>
                {
                    var res = new ResourceFile( resToPath );
                    res.Elements[updated.Key].Value.Should().Be( updated.Value ); // overwrite updated
                    res.Elements.Should().ContainSingle( el => el.Key == newFrom.Key && el.Value == newFrom.Value ); // copy new element
                    res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value ); // new element in dest remains unchanged
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        $"'{updated.Key}' element have been overwritten in '{resToPath}' file",
                        $"'{newFrom.Key}' element have been copied to '{resToPath}' file" );
                } );
        }
    }
}