using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using nresx.Tools.CodeParsers;
using NUnit.Framework;

namespace nresx.Core.Tests.CodeParsers
{
    [TestFixture]
    public class CsCodeParserTests : TestBase
    {
        [TestCase( @"        public string Description = ""The long description""; ", "TheFile", "TheFile_Description", "The long description" )]
        [TestCase( @"var description = ""The long description"";", "TheFile", "TheFile_Description", "The long description" )]
        
        [TestCase( @"string description => ""The long description"";", "TheFile", "TheFile_Description", "The long description" )]
        [TestCase( @"private static string Description => ""The long description""; ", "TheFile", "TheFile_Description", "The long description" )]

        [TestCase( @"var prop1 = obj2.TheMethod3( ""The long description"" );", "TheFile", "TheFile_TheLong", "The long description" )]
        
        public async Task ParseVariableDeclaration( string line, string elPath, string key, string value )
        {
            var result = new CsCodeParser().ParseLine( line, elPath );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>{ {key, value} } );
        }

        [Test]
        public async Task DuplicatedVariableHaveIndex()
        {
            var result = new CsCodeParser().ParseLine( @"var prop1 = obj2.TheMethod3( ""The text"", ""The text"" );", "TheFile" );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>
            {
                { "TheFile_TheText", "The text" },
                { "TheFile_TheText1", "The text" }
            } );
        }
    }
}
