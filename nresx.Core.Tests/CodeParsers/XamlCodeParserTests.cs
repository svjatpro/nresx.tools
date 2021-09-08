using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools.CodeParsers;
using NUnit.Framework;

namespace nresx.Core.Tests.CodeParsers
{
    [TestFixture]
    public class XamlCodeParserTests : TestBase
    {
        [TestCase( @"         <TextBlock Grid.Row=""0"" x:Name=""SampleTitle"" Text=""The title"" TextWrapping=""Wrap"" Margin=""0, 10, 0, 0"" FontSize=""28""/>", 
            "TheFile", "TheFile_SampleTitle.Text", "The title",
            @"         <TextBlock Grid.Row=""0"" x:Name=""SampleTitle"" x:Uid=""TheFile_SampleTitle"" TextWrapping=""Wrap"" Margin=""0, 10, 0, 0"" FontSize=""28""/>" )]
        [TestCase( @"         <TextBlock Grid.Row=""0"" Name=""SampleTitle"" Content=""The content"" TextWrapping=""Wrap"" Margin=""0, 10, 0, 0"" FontSize=""28""/>", 
            "TheFile", "TheFile_SampleTitle.Content", "The content",
            @"         <TextBlock Grid.Row=""0"" Name=""SampleTitle"" x:Uid=""TheFile_SampleTitle"" TextWrapping=""Wrap"" Margin=""0, 10, 0, 0"" FontSize=""28""/>" )]

        [TestCase( @"        <Button Grid.Row=""2"" Content=""The Button1""/>",
            "TheFile", "TheFile_Button_TheButton1.Content", "The Button1",
            @"        <Button Grid.Row=""2"" x:Uid=""TheFile_Button_TheButton1""/>" )]
        public async Task ParseVariableDeclaration( string line, string elPath, string key, string value, string replacedLine )
        {
            var newLine = new XamlCodeParser().ExtractFromLine( line, elPath, out var result );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>{ {key, value} } );

            newLine.Should().Be( replacedLine );
        }

        [Test]
        public async Task DuplicatedVariableHaveIndex()
        {
            var newLine = new XamlCodeParser().ExtractFromLine( @"<Button Grid.Row=""2"" Content=""The text""/> <Button Grid.Row=""3"" Content=""The text""/>", "TheFile", out var result );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>
            {
                { "TheFile_Button_TheText.Content", "The text" },
                { "TheFile_Button1_TheText.Content", "The text" }
            } );

            newLine.Should()
                .Be( @"<Button Grid.Row=""2"" x:Uid=""TheFile_Button_TheText""/> <Button Grid.Row=""3"" x:Uid=""TheFile_Button1_TheText""/>" );
        }
    }
}
