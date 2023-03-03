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
        [TestCase( @"        public string Description = ""The long description""; ", "TheFile", 
            "TheFile_Description", "The long description", @"        public string Description = GetString(""TheFile_Description""); " )]
        [TestCase( @"var description = ""The long description"";", "TheFile", 
            "TheFile_Description", "The long description", @"var description = GetString(""TheFile_Description"");" )]
        
        [TestCase( @"string description => ""The long description"";", "TheFile", 
            "TheFile_Description", "The long description", @"string description => GetString(""TheFile_Description"");" )]
        [TestCase( @"private static string Description => ""The long description""; ", "TheFile", 
            "TheFile_Description", "The long description", @"private static string Description => GetString(""TheFile_Description""); " )]

        [TestCase( @"var prop1 = obj2.TheMethod3( ""The long description"" );", "TheFile", 
            "TheFile_TheLong", "The long description", @"var prop1 = obj2.TheMethod3( GetString(""TheFile_TheLong"") );" )]
        
        public async Task ParseVariableDeclaration( string line, string elPath, string key, string value, string newLine )
        {
            string processedLine = null;
            var result = new Dictionary<string, string>();

            new CsCodeParser().ProcessNextLine( line, elPath,
                ( k, v ) => true,
                ( k, v ) => result.Add( k, v ),
                l => processedLine = l );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>{ {key, value} } );

            processedLine.Should().Be( newLine );
        }

        [Test]
        public async Task DuplicatedVariableHaveIndex()
        {
            string processedLine = null;
            var result = new Dictionary<string, string>();

            new CsCodeParser().ProcessNextLine( 
                @"var prop1 = obj2.TheMethod3( ""The text"", ""The text"" );", "TheFile",
                ( k, v ) => true,
                ( k, v ) => result.Add( k, v ),
                l => processedLine = l );
            foreach ( var keyValue in result )
                Console.WriteLine( $"{keyValue.Key}: {keyValue.Value}" );

            result.Should().BeEquivalentTo( new Dictionary<string, string>
            {
                { "TheFile_TheText", "The text" },
                { "TheFile_TheText1", "The text" }
            } );

            processedLine.Should().Be( @"var prop1 = obj2.TheMethod3( GetString(""TheFile_TheText""), GetString(""TheFile_TheText1"") );" );
        }
    }
}
