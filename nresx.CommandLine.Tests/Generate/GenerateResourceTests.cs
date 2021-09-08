using System.IO;
using FluentAssertions;
using nresx.Core.Tests;
using nresx.Tools;
using nresx.Tools.Extensions;
using NUnit.Framework;

namespace nresx.CommandLine.Tests.Generate
{
    [TestFixture]
    public class GenerateResourceTests : TestBase
    {
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile.resx] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [NewFile] -f resx --new-file" )]
        public void GenerateNewFile( string commandLine )
        {
            commandLine
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
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [TmpFile.resx] --new-file" )]
        [TestCase( @"generate -s [TmpProj.appUwp]\* -r -d [TmpFile] -f resx --new-file" )]
        public void AddToExistingFile( string commandLine )
        {
            commandLine
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
    }
}