using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Generate  
{
    [TestFixture]
    public class GenerateResourceTests : TestBase
    {
        #region Extract resources

        [TestCase( @"generate [TmpProj.appUwp]\* -d [NewFile] -r --new-file" )]
        [TestCase( @"generate [TmpProj.appUwp]\* -d [NewFile.resx] -f resx -r --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.resx] -f resx --new-file" )]
        public void GenerateNewFile( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_SampleTitle.Text" && el.Value == "The title" );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_Button_TheButton1.Content" && el.Value == "The Button1" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Description" && el.Value == "The long description" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Button2Content" && el.Value == "The Button2" );
                } )
                .ValidateDryRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.IsNewFile.Should().BeTrue();
                } )
                .ValidateStdout( args =>
                {
                    var dir = Path.GetFileName( args.TemporaryProjects[0] );
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        @$"""{dir}\MainPage.xaml"": ""The title"" string has been extracted to ""MainPage_SampleTitle.Text"" resource element",
                        @$"""{dir}\MainPage.xaml"": ""The Button1"" string has been extracted to ""MainPage_Button_TheButton1.Content"" resource element",
                        @$"""{dir}\MainViewModel.cs"": ""The long description"" string has been extracted to ""MainViewModel_Description"" resource element",
                        @$"""{dir}\MainViewModel.cs"": ""The Button2"" string has been extracted to ""MainViewModel_Button2Content"" resource element" );
                } );
        }

        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [TmpFile] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [TmpFile.resx] -f resx --new-file" )]
        public void AddToExistingFile( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_SampleTitle.Text" && el.Value == "The title" );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_Button_TheButton1.Content" && el.Value == "The Button1" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Description" && el.Value == "The long description" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Button2Content" && el.Value == "The Button2" );
                } )
                .ValidateDryRun( args =>
                {
                    var res = new ResourceFile( args.TemporaryFiles[0] );
                    res.Elements.Should().NotContain( el => el.Key == "MainPage_SampleTitle.Text" && el.Value == "The title" );
                    res.Elements.Should().NotContain( el => el.Key == "MainPage_Button_TheButton1.Content" && el.Value == "The Button1" );
                    res.Elements.Should().NotContain( el => el.Key == "MainViewModel_Description" && el.Value == "The long description" );
                    res.Elements.Should().NotContain( el => el.Key == "MainViewModel_Button2Content" && el.Value == "The Button2" );
                } )
                .ValidateStdout( args =>
                {
                    var dir = Path.GetFileName( args.TemporaryProjects[0] );
                    args.ConsoleOutput.Should().BeEquivalentTo(
                        @$"""{dir}\MainPage.xaml"": ""The title"" string has been extracted to ""MainPage_SampleTitle.Text"" resource element",
                        @$"""{dir}\MainPage.xaml"": ""The Button1"" string has been extracted to ""MainPage_Button_TheButton1.Content"" resource element",
                        @$"""{dir}\MainViewModel.cs"": ""The long description"" string has been extracted to ""MainViewModel_Description"" resource element",
                        @$"""{dir}\MainViewModel.cs"": ""The Button2"" string has been extracted to ""MainViewModel_Button2Content"" resource element" );
                } );
        }

        #endregion

        #region Link with generated resources

        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.po] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.json] -f json --new-file" )]
        public void DoNotLinkByDefault( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var dir = args.TemporaryProjects[0];

                    File.ReadAllText( $"{dir}\\MainPage.xaml" ).Should()
                        .NotContain( "x:Uid=\"MainPage_SampleTitle\"" ).And
                        .NotContain( "x:Uid=\"MainPage_Button_TheButton1\"" );
                    File.ReadAllText( $"{dir}\\MainViewModel.cs" ).Should()
                        .NotContain( "MainViewModel_Description" ).And
                        .NotContain( "MainViewModel_Button2Content" );
                } )
                .ValidateDryRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.IsNewFile.Should().BeTrue();
                } );
        }
        
        [TestCase( @"generate -s [TmpProj.appUwp]\* -d [NewFile.po] -r --link --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -d [NewFile.json] -f json -r --link --new-file" )]
        public void LinkWithGeneratedResources( string commandLine )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var dir = args.TemporaryProjects[0];

                    File.ReadAllText( $"{dir}\\MainPage.xaml" ).Should()
                        .Contain( "x:Uid=\"MainPage_SampleTitle\"" ).And
                        .Contain( "x:Uid=\"MainPage_Button_TheButton1\"" );
                    File.ReadAllText( $"{dir}\\MainViewModel.cs" ).Should()
                        .Contain( "MainViewModel_Description" ).And
                        .Contain( "MainViewModel_Button2Content" );
                } )
                .ValidateDryRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.IsNewFile.Should().BeTrue();
                } );
        }

        #endregion

        #region Skip already linked resources

        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.po] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.json] -f json --new-file" )]
        public void DoNotDuplicateResourceElements( string commandLine )
        {
            var argsFirst = TestHelper.RunCommandLine( 
                commandLine, 
                options: new CommandRunOptions { SkipFilesWithoutKey = true, MergeArgs = true } );

            commandLine
                .PrepareArgs( () => argsFirst )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.Elements.Should().HaveCount( 4 );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_SampleTitle.Text" && el.Value == "The title" );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_Button_TheButton1.Content" && el.Value == "The Button1" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Description" && el.Value == "The long description" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Button2Content" && el.Value == "The Button2" );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEmpty();
                } );
        }

        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.po] --new-file --link" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.json] -f json --new-file --link" )]
        public void DoNotOverwriteLinkedResources( string commandLine )
        {
            var argsFirst = TestHelper.RunCommandLine(
                commandLine,
                options: new CommandRunOptions { SkipFilesWithoutKey = true, MergeArgs = true } );

            commandLine
                .PrepareArgs( () => argsFirst )
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.Elements.Should().HaveCount( 4 );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_SampleTitle.Text" && el.Value == "The title" );
                    res.Elements.Should().Contain( el => el.Key == "MainPage_Button_TheButton1.Content" && el.Value == "The Button1" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Description" && el.Value == "The long description" );
                    res.Elements.Should().Contain( el => el.Key == "MainViewModel_Button2Content" && el.Value == "The Button2" );

                    var dir = args.TemporaryProjects[0];
                    File.ReadAllText( $"{dir}\\MainPage.xaml" ).Should()
                        .Contain( "x:Uid=\"MainPage_SampleTitle\"" ).And
                        .Contain( "x:Uid=\"MainPage_Button_TheButton1\"" );
                    File.ReadAllText( $"{dir}\\MainViewModel.cs" ).Should()
                        .Contain( "\"MainViewModel_Description\"" ).And
                        .Contain( "\"MainViewModel_Button2Content\"" );
                } )
                .ValidateStdout( args =>
                {
                    args.ConsoleOutput.Should().BeEmpty();
                } );
        }

        #endregion

        #region Exclude dir

        [TestCase( @"generate [TmpProj.appUwp]\* -d [NewFile] -r --new-file --exclude ""obj""", 4 )]
        [TestCase( @"generate [TmpProj.appUwp]\* -d [NewFile] -r --new-file --exclude ""obj2""", 6 )]
        public void ExcludeDir( string commandLine, int resourceCount )
        {
            commandLine
                .WithOptions( opt => opt.SkipFilesWithoutKey = true )
                .ValidateRun( args =>
                {
                    var res = new ResourceFile( args.NewFiles[0] );
                    res.Elements.Should().HaveCount( resourceCount );
                } );
        }

        #endregion
    }
}