using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Helpers;

namespace nresx.Core.Tests
{
    public class TestBase
    {
        private readonly string TestFileFolder = ".test_files";
        private readonly string OutputFileFolder = ".test_output";

        protected string GetOutputPath( string fileName, ResourceFormatType type )
        {
            if ( ResourceFormatHelper.DetectExtension( type, out var extension ) )
            {
                var path = Path.Combine( OutputFolder, fileName );
                path = Path.ChangeExtension( path, extension );
                return path;
            }
            return null;
        }

        protected void Run( string cmdLine )
        {
            var args = cmdLine
                .Replace( "[Files]", TestFileFolder )
                .Replace( "[Output]", OutputFolder );
            var cmd = $"/C nresx {args}";
            var result = System.Diagnostics.Process.Start( "CMD.exe", cmd );
            result?.WaitForExit( 5000 );
        }

        protected void Run( string cmdLine, out string key )
        {
            key = UniqueKey();
            var args = cmdLine
                .Replace( "[Files]", TestFileFolder )
                .Replace( "[Output]", OutputFolder )
                .Replace( "[UniqueKey]", $"{key}" );
            var cmd = $"/C nresx {args}";
            var result = System.Diagnostics.Process.Start( "CMD.exe", cmd );
            result?.WaitForExit( 5000 );
        }

        protected string OutputFolder
        {
            get
            {
                var dir = new DirectoryInfo( OutputFileFolder );
                if ( !dir.Exists )
                    dir.Create();

                return OutputFileFolder;
            }
        }
        
        protected string GetTestPath( string path )
        {
            return $".test_files\\{path}";
        }

        protected void ValidateElements( ResourceFile resource )
        {
            var elements = resource.Elements.ToList();

            elements
                .Select( e => (key: e.Key, val: e.Value) )
                .Should().BeEquivalentTo(
                    (key: "Entry1.Text", val: "Value1"),
                    (key: "Entry2", val: "Value2"),
                    (key: "Entry3", val: "Value3\r\nmultiline") );
        }

        protected string UniqueKey( int length = 8 )
        {
            var key = Convert.ToBase64String( Guid.NewGuid().ToByteArray() )
                .Replace( "+", "" )
                .Replace( "/", "" )
                .Replace( "=", "" );
            return key.Substring( 0, Math.Min( length, key.Length ) );
        }

        protected ResourceFile GetExampleResourceFile()
        {
            var example = new ResourceFile( GetTestPath( "Resources.resx" ) );
            return example;
        }

        protected void AddExampleElements( ResourceFile res )
        {
            var example = GetExampleResourceFile();
            foreach ( var el in example.Elements )
                res.AddElement( el.Key, el.Value, el.Comment );
        }
    }
}