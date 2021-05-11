using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.Core.Tests
{
    public class TestBase
    {
        public static void CleanOutputDir()
        {
            void RemoveFiles( DirectoryInfo dir, bool removeDir )
            {
                foreach ( var fileInfo in dir.GetFiles() )
                    fileInfo.Delete();
                foreach ( var subDir in dir.GetDirectories() )
                    RemoveFiles( subDir, true );
                
                if( removeDir )
                    dir.Delete();
            }

            var root = new DirectoryInfo( TestData.OutputFolder );
            if ( !root.Exists )
            {
                root.Create();
            }
            else
            {
                RemoveFiles( root, false );
            }
        }

        protected string UniqueKey( int length = 8 )
        {
            var key = Convert.ToBase64String( Guid.NewGuid().ToByteArray() )
                .Replace( "+", "" )
                .Replace( "/", "" )
                .Replace( "=", "" );
            return key.Substring( 0, Math.Min( length, key.Length ) );
        }

        protected string GetTestPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
        {
            var path = Path.Combine( TestData.TestFileFolder, fileName );
            if ( !Path.HasExtension( path ) && ResourceFormatHelper.DetectExtension( type, out var extension ) )
                path = Path.ChangeExtension( path, extension );
            return path;
        }

        protected string GetOutputPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
        {
            var path = Path.Combine( TestData.OutputFolder, fileName );
            if ( !Path.HasExtension( path ) && ResourceFormatHelper.DetectExtension( type, out var extension ) )
                path = Path.ChangeExtension( path, extension );
            return path;
        }

        protected string PrepareCommandLine( 
            string cmdLine, 
            out CommandLineParameters parameters,
            CommandLineParameters predefinedParams = null )
        {
            var resultParams = new CommandLineParameters();
            var resultCmdLine = new StringBuilder( cmdLine );

            // replace static params
            resultCmdLine
                .Replace( CommandLineTags.FilesDir, TestData.TestFileFolder )
                .Replace( CommandLineTags.OutputDir, TestData.OutputFolder );

            void ReplaceTags( string tagPlaceholder, Func<ResourceFormatType, string> getTagValue )
            {
                var tagName = tagPlaceholder.TrimStart( ' ', '[' ).TrimEnd( ' ', ']' );
                var regx = new Regex( $"\\[{tagName}(.[\\w]+|)\\]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
                var matches = regx.Matches( resultCmdLine.ToString() );

                foreach ( Match match in matches )
                {
                    string tag;
                    var formatType = ResourceFormatType.Resx;
                    if ( match.Groups.Count > 1 && !string.IsNullOrWhiteSpace( match.Groups[1].Value ) )
                    {
                        var ext = match.Groups[1].Value;
                        if ( ResourceFormatHelper.DetectFormatByExtension( ext, out var t ) )
                            formatType = t;
                        tag = $"[{tagName}{match.Groups[1].Value}]";
                    }
                    else
                    {
                        tag = $"[{tagName}]";
                    }
                    
                    for ( int p = 0, i = resultCmdLine.ToString().IndexOf( tag, StringComparison.InvariantCulture );
                          i >= 0;
                          p += i, i = resultCmdLine.ToString( p, resultCmdLine.Length - p ).IndexOf( tag, StringComparison.Ordinal ) )
                    {
                        resultCmdLine.Replace( tag, getTagValue( formatType ), p + i, tag.Length );
                    }
                }
            }

            // get resource files and replace its paths
            ReplaceTags(
                CommandLineTags.SourceFile,
                type =>
                {
                    if ( predefinedParams != null && predefinedParams.SourceFiles.TryTake( out var p ) )
                        return p;

                    var path = GetTestPath( TestData.ExampleResourceFile, type );
                    resultParams.SourceFiles.Add( path );
                    return path;
                } );

            // generate output files paths and replace in commant line
            ReplaceTags(
                CommandLineTags.DestFile,
                type =>
                { 
                    if ( predefinedParams != null && predefinedParams.DestinationFiles.TryTake( out var p ) )
                        return p;
                    
                    var path = GetOutputPath( UniqueKey(), type );
                    resultParams.DestinationFiles.Add( path );
                    return path;
                } );

            // generate temporary files and replace its paths
            ReplaceTags(
                CommandLineTags.TemporaryFile,
                type =>
                {
                    if ( predefinedParams != null && predefinedParams.TemporaryFiles.TryTake( out var p ) )
                        return p;

                    var destPath = CopyTemporaryFile( copyType: type );
                    resultParams.TemporaryFiles.Add( destPath );
                    return destPath;
                } );
            
            // generate unique key(s) and replace in command line
            ReplaceTags(
                CommandLineTags.UniqueKey,
                type =>
                {
                    if ( predefinedParams != null && predefinedParams.UniqueKeys.TryTake( out var p ) )
                        return p;

                    var key = UniqueKey();
                    resultParams.UniqueKeys.Add( key );
                    return key;
                } );

            parameters = resultParams;
            return resultCmdLine.ToString();
        }

        protected CommandLineParameters Run( string cmdLine, CommandLineParameters parameters = null )
        {
            var args = PrepareCommandLine( cmdLine, out var p, parameters );

            var cmd = $"/C nresx {args}";
            var process = new Process();
            process.StartInfo = new ProcessStartInfo( "CMD.exe", cmd );
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            process.WaitForExit( 5000 );

            while ( !process.StandardOutput.EndOfStream )
                p.ConsoleOutput.Add( process.StandardOutput.ReadLine() );

            return p;
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
        
        protected ResourceFile GetExampleResourceFile()
        {
            var example = new ResourceFile( GetTestPath( TestData.ExampleResourceFile ) );
            return example;
        }

        protected string CopyTemporaryFile( 
            string sourcePath = null, 
            string destPath = null,
            ResourceFormatType copyType = ResourceFormatType.Resx )
        {
            var key = UniqueKey();
            if( string.IsNullOrWhiteSpace( destPath ) )
                destPath = GetOutputPath( key, copyType );

            var resx = new ResourceFile( sourcePath ?? GetTestPath( TestData.ExampleResourceFile ) );
            resx.Save( destPath, copyType );

            return destPath;
        }

        protected void AddExampleElements( ResourceFile res )
        {
            var example = GetExampleResourceFile();
            foreach ( var el in example.Elements )
                res.Elements.Add( el.Key, el.Value, el.Comment );
        }
    }
}