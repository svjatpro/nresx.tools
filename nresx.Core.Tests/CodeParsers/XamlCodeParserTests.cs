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
        [TestCase( @"         <TextBlock Grid.Row=""0"" x:Name=""SampleTitle"" Text=""The title"" TextWrapping=""Wrap"" Margin=""0, 10, 0, 0"" FontSize=""28""/>", "TheFile", "TheFile_SampleTitle.Text", "The title" )]
        [TestCase( @"         <TextBlock Grid.Row=""0"" Name=""SampleTitle"" Content=""The content"" TextWrapping=""Wrap"" Margin=""0, 10, 0, 0"" FontSize=""28""/>", "TheFile", "TheFile_SampleTitle.Content", "The content" )]

        [TestCase( @"        <Button Grid.Row=""2"" Content=""The Button1""/>", "TheFile", "TheFile_Button_TheButton1.Content", "The Button1" )]
        public async Task ParseVariableDeclaration( string line, string elPath, string key, string value )
        {
            var result = new XamlCodeParser().ParseLine( line, elPath );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>{ {key, value} } );
        }

        [Test]
        public async Task DuplicatedVariableHaveIndex()
        {
            var result = new XamlCodeParser().ParseLine( @"<Button Grid.Row=""2"" Content=""The text""/> <Button Grid.Row=""3"" Content=""The text""/>", "TheFile" );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>
            {
                { "TheFile_Button_TheText.Content", "The text" },
                { "TheFile_Button1_TheText.Content", "The text" }
            } );
        }
    }
}
