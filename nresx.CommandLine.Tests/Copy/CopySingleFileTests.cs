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
            string resFromPath, string resToPath,
            out ResourceElement updated, out ResourceElement newFrom, out ResourceElement newTo )
        {
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
        
        [TestCase( @"copy [TmpFile] [NewFile]" )]
        [TestCase( @"copy [TmpFile.resx] [NewFile.yaml]" )]
        [TestCase( @"copy [TmpFile] -d [NewFile]" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile]" )]
        public void CopyToNonExistingFile( string commandLine )
        {
            var args = Run( commandLine );

            new FileInfo( args.NewFiles[0] ).Exists.Should().BeFalse();
            args.ConsoleOutput.Should().BeEquivalentTo( string.Format( FilesNotFoundErrorMessage, args.NewFiles[0].GetShortPath() ) );
        }
        
        [TestCase( @"copy [TmpFile] [NewFile] --new-file" )]
        [TestCase( @"copy [TmpFile.resx] [NewFile.yaml] --new-file" )]
        [TestCase( @"copy [TmpFile] -d [NewFile] --new-file" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile] --new-file" )]
        public void CopyToNonExistingFileShouldCreateOne( string commandLine )
        {
            var args = Run( commandLine );
            
            var res = new ResourceFile( args.NewFiles[0] );
            ValidateElements( res );
        }

        [TestCase( @"copy [TmpFile] [NewFile] --new-file --dry-run" )]
        [TestCase( @"copy [TmpFile.resx] [NewFile.yaml] --new-file --dry-run" )]
        [TestCase( @"copy [TmpFile] -d [NewFile] --new-file --dry-run" )]
        [TestCase( @"copy -s [TmpFile] -d [NewFile] --new-file --dry-run" )]
        public void CopyToNonExistingFileShouldCreateOne_DryRun( string commandLine )
        {
            var args = Run( commandLine );

            new FileInfo( args.NewFiles[0] ).Exists.Should().BeFalse();

            var res = new ResourceFile( args.TemporaryFiles[0] );
            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"'{res.Elements[0].Key}' element have been copied to '{args.NewFiles[0]}' file",
                $"'{res.Elements[1].Key}' element have been copied to '{args.NewFiles[0]}' file",
                $"'{res.Elements[2].Key}' element have been copied to '{args.NewFiles[0]}' file" );
        }


        
        [TestCase( @"copy [TmpFile] [TmpFile] --skip" )]
        [TestCase( @"copy [TmpFile] [TmpFile]" )]
        [TestCase( @"copy [TmpFile.resx] [TmpFile.yaml] --skip" )]
        [TestCase( @"copy [TmpFile] -d [TmpFile] --skip" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --skip" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile]" )]
        public void CopyToExistingFileShouldSkipDuplicates( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var prepArgs );
            var resFromPath = prepArgs.TemporaryFiles[0];
            var resToPath = prepArgs.TemporaryFiles[1];
            PrepareFromToResources( resFromPath, resToPath, out var updated, out var newFrom, out var newTo );

            var args = Run( commandLine, prepArgs );

            var res = new ResourceFile( resToPath );
            res.Elements[updated.Key].Value.Should().NotBe( updated.Value ); // skip updated
            res.Elements.Should().ContainSingle( el => el.Key == newFrom.Key && el.Value == newFrom.Value ); // copy new element
            res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value ); // new element in dest remains unchanged

            args.ConsoleOutput.Should().BeEquivalentTo( $"'{newFrom.Key}' element have been copied to '{resToPath}' file" );
        }

        [TestCase( @"copy [TmpFile] [TmpFile] --dry-run" )]
        [TestCase( @"copy [TmpFile] [TmpFile] --skip --dry-run" )]
        [TestCase( @"copy [TmpFile.resx] [TmpFile.yaml] --skip --dry-run" )]
        [TestCase( @"copy [TmpFile] -d [TmpFile] --skip --dry-run" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --skip --dry-run" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --dry-run" )]
        public void CopyToExistingFileShouldSkipDuplicates_DryRun( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var prepArgs );
            var resFromPath = prepArgs.TemporaryFiles[0];
            var resToPath = prepArgs.TemporaryFiles[1];
            PrepareFromToResources( resFromPath, resToPath, out var updated, out var newFrom, out var newTo );

            var args = Run( commandLine, prepArgs );

            var res = new ResourceFile( resToPath );
            res.Elements[updated.Key].Value.Should().NotBe( updated.Value );
            res.Elements.Should().NotContain( el => el.Key == newFrom.Key );
            res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value );

            args.ConsoleOutput.Should().BeEquivalentTo( $"'{newFrom.Key}' element have been copied to '{resToPath}' file" );
        }



        [TestCase( @"copy [TmpFile] [TmpFile] --overwrite" )]
        [TestCase( @"copy [TmpFile.resx] [TmpFile.yaml] --overwrite" )]
        [TestCase( @"copy [TmpFile] -d [TmpFile] --overwrite" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --overwrite" )]
        public void CopyToExistingFileShouldOverwriteDuplicates( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var prepArgs );
            var resFromPath = prepArgs.TemporaryFiles[0];
            var resToPath = prepArgs.TemporaryFiles[1];
            PrepareFromToResources( resFromPath, resToPath, out var updated, out var newFrom, out var newTo );

            var args = Run( commandLine, prepArgs );

            var res = new ResourceFile( resToPath );
            res.Elements[updated.Key].Value.Should().Be( updated.Value ); // overwrite updated
            res.Elements.Should().ContainSingle( el => el.Key == newFrom.Key && el.Value == newFrom.Value ); // copy new element
            res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value ); // new element in dest remains unchanged

            args.ConsoleOutput.Should().BeEquivalentTo( 
                $"'{updated.Key}' element have been overwritten in '{resToPath}' file",
                $"'{newFrom.Key}' element have been copied to '{resToPath}' file" );
        }

        [TestCase( @"copy [TmpFile] [TmpFile] --overwrite --dry-run" )]
        [TestCase( @"copy [TmpFile.resx] [TmpFile.yaml] --overwrite --dry-run" )]
        [TestCase( @"copy [TmpFile] -d [TmpFile] --overwrite --dry-run" )]
        [TestCase( @"copy -s [TmpFile] -d [TmpFile] --overwrite --dry-run" )]
        public void CopyToExistingFileShouldOverwriteDuplicates_DryRun( string commandLine )
        {
            TestHelper.PrepareCommandLine( commandLine, out var prepArgs );
            var resFromPath = prepArgs.TemporaryFiles[0];
            var resToPath = prepArgs.TemporaryFiles[1];
            PrepareFromToResources( resFromPath, resToPath, out var updated, out var newFrom, out var newTo );

            var args = Run( commandLine, prepArgs );

            var res = new ResourceFile( resToPath );
            res.Elements[updated.Key].Value.Should().NotBe( updated.Value );
            res.Elements.Should().NotContain( el => el.Key == newFrom.Key );
            res.Elements.Should().ContainSingle( el => el.Key == newTo.Key && el.Value == newTo.Value );

            args.ConsoleOutput.Should().BeEquivalentTo(
                $"'{updated.Key}' element have been overwritten in '{resToPath}' file",
                $"'{newFrom.Key}' element have been copied to '{resToPath}' file" );
        }

    }
}