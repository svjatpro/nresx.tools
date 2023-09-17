using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.Core.Tests
{
    public class CommandRunOptions
    {
        public bool MergeArgs { get; set; } = false;
        public bool SkipFilesWithoutKey { get; set; } = false;
        public bool SkipFilesWithoutComment { get; set; } = false;

        public string WorkingDirectory { get; set; }
    }

    public class TestHelper
    {
        private static void ReplaceTags( 
            StringBuilder resultCmdLine, 
            string tagPlaceholder, 
            Func<ResourceFormatType, string, string, string> getTagValue,
            CommandRunOptions options = null )
        {
            const string dirPlaceholder = @"Dir\";
            var tagName = tagPlaceholder.TrimStart( ' ', '[' ).TrimEnd( ' ', ']' );
            var regx = new Regex( $"\\[(Dir\\\\|){tagName}(.[\\w]+|)\\]", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
            var matches = regx.Matches( resultCmdLine.ToString() );

            foreach ( Match match in matches )
            {
                string tag;
                var formatType = TestData.GetRandomType( options );
                var param = "";
                var dir = "";
                var dirPrefix = match.Groups[1].Value;
                if ( match.Groups.Count > 1 && dirPrefix == dirPlaceholder )
                {
                    dir = $"{TestData.UniqueKey()}\\";
                    new DirectoryInfo( Path.Combine( TestData.OutputFolder, dir ) ).Create();
                }

                if ( match.Groups.Count > 2 && !string.IsNullOrWhiteSpace( match.Groups[2].Value ) )
                {
                    var ext = match.Groups[2].Value;
                    param = ext.TrimStart( '.' );
                    if ( ResourceFormatHelper.DetectFormatByExtension( ext, out var t ) )
                        formatType = t;
                    tag = $"[{dirPrefix}{tagName}{ext}]";
                }
                else
                {
                    tag = $"[{dirPrefix}{tagName}]";
                }

                for ( int p = 0, i = resultCmdLine.ToString().IndexOf( tag, StringComparison.InvariantCulture );
                    i >= 0;
                    p += i, i = resultCmdLine.ToString( p, resultCmdLine.Length - p ).IndexOf( tag, StringComparison.Ordinal ) )
                {
                    resultCmdLine.Replace( tag, getTagValue( formatType, dir, param ), p + i, tag.Length );
                }
            }
        }

        public static string GetTestPath( string fileName, ResourceFormatType type = ResourceFormatType.NA )
        {
            var path = Path.Combine( TestData.TestFileFolder, fileName );
            if ( !Path.HasExtension( path ) && type != ResourceFormatType.NA )
                type = ResourceFormatType.Resx;

            if ( type != ResourceFormatType.NA && ResourceFormatHelper.DetectExtension( type, out var extension ) )
                path = Path.ChangeExtension( path, extension );
            return path;
        }

        public static string GetOutputPath( string fileName, ResourceFormatType type = ResourceFormatType.Resx )
        {
            var path = Path.Combine( TestData.OutputFolder, fileName );
            if ( !Path.HasExtension( path ) && ResourceFormatHelper.DetectExtension( type, out var extension ) )
                path = Path.ChangeExtension( path, extension );
            return path;
        }

        public static string CopyTemporaryFile(
            string sourcePath = null,
            string destPath = null,
            string destDir = null,
            ResourceFormatType copyType = ResourceFormatType.Resx )
        {
            var key = TestData.UniqueKey();
            if ( string.IsNullOrWhiteSpace( destPath ) )
            {
                var file = string.IsNullOrWhiteSpace( destDir ) ? key : Path.Combine( destDir, key );
                destPath = GetOutputPath( file, copyType );
            }

            var resx = new ResourceFile( sourcePath ?? GetTestPath( TestData.ExampleResourceFile ) );
            resx.Save( destPath, copyType, createDir: true );

            return destPath;
        }


        public static void ReplaceKey( string path, string key, string newValue )
        {
            var lines = new StringBuilder();
            using ( var reader = new StreamReader( new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) ) )
            {
                while ( !reader.EndOfStream )
                    lines.AppendLine( reader.ReadLine()?.Replace( key, newValue ) );
            }
            using var writer = new StreamWriter( new FileStream( path, FileMode.Create, FileAccess.Write, FileShare.Write ) );
            var content = new MemoryStream( Encoding.UTF8.GetBytes( lines.ToString() ) );
            using ( var reader = new StreamReader( content ) )
            {
                while ( !reader.EndOfStream )
                    writer.WriteLine( reader.ReadLine() );
            }
        }

        public static string PrepareCommandLine(
            string cmdLine,
            out CommandLineParameters parameters,
            CommandLineParameters predefinedParams = null,
            CommandRunOptions options = null )
        {
            if ( options == null )
                options = new CommandRunOptions();

            var resultParams = new CommandLineParameters();
            var resultCmdLine = new StringBuilder( cmdLine );

            // replace static params
            resultCmdLine
                .Replace( CommandLineTags.FilesDir, TestData.TestFileFolder )
                .Replace( CommandLineTags.OutputDir, TestData.OutputFolder );

            // get resource files and replace its paths
            ReplaceTags(
                resultCmdLine, CommandLineTags.SourceFile,
                ( type, _, _ ) =>
                {
                    if ( predefinedParams != null && predefinedParams.SourceFiles.TryTake( out var p ) )
                    {
                        if( options.MergeArgs )
                            resultParams.SourceFiles.Add( p );
                        return p;
                    }

                    var path = GetTestPath( Path.ChangeExtension( TestData.ExampleResourceFile, "" ), type );
                    resultParams.SourceFiles.Add( path );
                    return path;
                },
                options );

            // generate output files paths and replace in command line
            ReplaceTags(
                resultCmdLine, CommandLineTags.NewFile,
                ( type, dir, _ ) =>
                {
                    if ( predefinedParams != null && predefinedParams.NewFiles.TryTake( out var p ) )
                    {
                        if( options.MergeArgs )
                            resultParams.NewFiles.Add( p );
                        return p;
                    }

                    var file = string.IsNullOrWhiteSpace( dir ) ? TestData.UniqueKey() : Path.Combine( dir, TestData.UniqueKey() );
                    var path = GetOutputPath( file, type );
                    resultParams.NewFiles.Add( path );
                    return path;
                },
                options );

            // generate temporary files and replace its paths
            ReplaceTags(
                resultCmdLine, CommandLineTags.TemporaryFile,
                ( type, dir, _ ) =>
                {
                    if ( predefinedParams != null && predefinedParams.TemporaryFiles.TryTake( out var p ) )
                    {
                        if ( options.MergeArgs )
                            resultParams.TemporaryFiles.Add( p );
                        return p;
                    }

                    var destPath = CopyTemporaryFile( copyType: type, destDir: dir );
                    resultParams.TemporaryFiles.Add( destPath );
                    return destPath;
                },
                options);

            // generate unique key(s) and replace in command line
            ReplaceTags(
                resultCmdLine, CommandLineTags.UniqueKey,
                ( _, _, _ ) =>
                {
                    if ( predefinedParams != null && predefinedParams.UniqueKeys.TryTake( out var p ) )
                    {
                        if( options.MergeArgs )
                            resultParams.UniqueKeys.Add( p );
                        return p;
                    }

                    var key = TestData.UniqueKey();
                    resultParams.UniqueKeys.Add( key );
                    return key;
                },
                options );

            // generate unique key(s) and replace in command line
            ReplaceTags(
                resultCmdLine, CommandLineTags.RandomExtension,
                ( type, _, _ ) =>
                {
                    var ext = ResourceFormatHelper.GetExtension( type );
                    resultParams.RandomExtensions.Add( ext );
                    return ext;
                },
                options );

            // create new directory
            ReplaceTags(
                resultCmdLine, CommandLineTags.NewDir,
                ( _, dir, _ ) =>
                {
                    if ( predefinedParams != null && predefinedParams.NewDirectories.TryTake( out var p ) )
                    {
                        if( options.MergeArgs )
                            resultParams.NewDirectories.Add( p );
                        return p;
                    }

                    var newDir = string.IsNullOrWhiteSpace( dir ) ? $"{TestData.UniqueKey()}" : Path.Combine( dir, TestData.UniqueKey() );
                    var dirInfo = new DirectoryInfo( Path.Combine( TestData.OutputFolder, newDir ) );
                    if ( !dirInfo.Exists )
                        dirInfo.Create();

                    //var destPath = CopyTemporaryFile( copyType: type, destDir: dir );
                    resultParams.NewDirectories.Add( newDir );
                    return newDir;
                },
                options );

            // create copy of the project in temporary output directory
            ReplaceTags(
                resultCmdLine, CommandLineTags.TemporaryProjectDir,
                ( _, _, parameter ) =>
                {
                    if ( predefinedParams != null && predefinedParams.TemporaryProjects.TryTake( out var p ) )
                    {
                        if ( options.MergeArgs )
                            resultParams.TemporaryProjects.Add( p );
                        return p;
                    }

                    var projDir = Path.Combine( TestData.ProjectsFolder, parameter );
                    var targetDir = Path.Combine( TestData.OutputFolder, $"{parameter}_{TestData.UniqueKey()}" );
                    FilesHelper.CopyDirectory( projDir, targetDir );

                    resultParams.TemporaryProjects.Add( targetDir );
                    return targetDir;
                },
                options );

            var result = resultCmdLine.ToString();

            resultParams.CommandLine = result;
            resultParams.DryRun = result.Contains( TestData.DryRunOption );
            resultParams.Recursive = result.Contains( TestData.RecursiveOption ) || result.Contains( TestData.RecursiveShortOption );

            if ( options.MergeArgs )
            {
                if( predefinedParams?.SourceFiles.Any() ?? false )
                    resultParams.SourceFiles.AddRange( predefinedParams.SourceFiles );
                if ( predefinedParams?.NewFiles.Any() ?? false )
                    resultParams.NewFiles.AddRange( predefinedParams.NewFiles );
                if ( predefinedParams?.TemporaryFiles.Any() ?? false )
                    resultParams.TemporaryFiles.AddRange( predefinedParams.TemporaryFiles );
                if ( predefinedParams?.UniqueKeys.Any() ?? false )
                    resultParams.UniqueKeys.AddRange( predefinedParams.UniqueKeys );
                if ( predefinedParams?.RandomExtensions.Any() ?? false )
                    resultParams.RandomExtensions.AddRange( predefinedParams.RandomExtensions );
                if ( predefinedParams?.NewDirectories.Any() ?? false )
                    resultParams.NewDirectories.AddRange( predefinedParams.NewDirectories );
                if ( predefinedParams?.TemporaryProjects.Any() ?? false )
                    resultParams.TemporaryProjects.AddRange( predefinedParams.TemporaryProjects );
            }

            parameters = resultParams;
            return result;
        }

        public static CommandLineParameters RunCommandLine( 
            string cmdLine, 
            CommandLineParameters parameters = null,
            CommandRunOptions options = null )
        {
            var debugCommandLine = false;
#if DEBUG
            bool.TryParse( Environment.GetEnvironmentVariable( "DEBUG_COMMAND_LINE" ), out debugCommandLine );
#endif

            var args = PrepareCommandLine( cmdLine, out var p, parameters, options );
            if( debugCommandLine )
                args += " --debug";

            var process = new Process();
            
            process.StartInfo = new ProcessStartInfo( "nresx", args );
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            if ( !string.IsNullOrWhiteSpace( options?.WorkingDirectory ) ) 
                process.StartInfo.WorkingDirectory = options.WorkingDirectory;

            process.Start();
            if ( debugCommandLine )
                process.WaitForExit();
            else
                process.WaitForExit( 5000 );

            p.ExitCode = process.ExitCode;

            while ( !process.StandardOutput.EndOfStream )
            {
                var line = process.StandardOutput.ReadLine();
                p.ConsoleOutput.Add( line );
            }

            Console.WriteLine( $@"============ command line run: =============" );
            Console.WriteLine( $@"nresx {args}" );
            Console.WriteLine( new string( '=', 50 ) );
            foreach ( var line in p.ConsoleOutput )
                Console.WriteLine( line );
            Console.WriteLine( new string( '=', 50 ) );
            Console.WriteLine();
            
            return p;
        }
    }
}