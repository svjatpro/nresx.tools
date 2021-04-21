using CommandLine;
using nresx.CommandLine.Commands;

namespace nresx.CommandLine
{
    class Program
    {
        //private static List<string> GetFiles( string root, string pattern )
        //{
        //    var dirSkipList = new HashSet<string>( StringComparer.CurrentCultureIgnoreCase )
        //    {
        //        ".git", ".vs", "bin", "obj"
        //    };

        //    var resFiles = new List<string>();

        //    var files = Directory.EnumerateFiles( root, pattern ).ToList();
        //    if(files.Any())
        //        resFiles.AddRange( files );

        //    foreach ( var dirPath in Directory.GetDirectories( root ) )
        //    { 
        //        var dirName = Path.GetFileName( dirPath );
        //        if ( dirSkipList.Contains( dirName ) ) continue;

        //        var dirFiles = GetFiles( dirPath, pattern );
        //        if( dirFiles.Any() )
        //            resFiles.AddRange( dirFiles );
        //    }  

        //    return resFiles;
        //}

        //private static List<string> GetResReferencesXaml( string path )
        //{
        //    //x:Uid="SettingsPage_Title"
        //    Regex resRegex = new( @"x:Uid\s*=\s*""([a-zA-Z0-9_]+)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

        //    using var reader = new StreamReader( new FileStream( path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );

        //    var refs = new List<string>();
        //    while ( !reader.EndOfStream )
        //    {
        //        var line = reader.ReadLine();
        //        if ( string.IsNullOrWhiteSpace( line ) )
        //            continue;
        //        var match = resRegex.Match( line );
        //        if ( match.Success && match.Groups.Count > 1 )
        //        {
        //            refs.AddRange( match.Groups.Values.Skip( 1 ).Select( g => g.Value ) );
        //            //refs.Add( await reader.ReadLineAsync() );
        //        }
        //    }

        //    return refs;
        //}

        static void Main( string[] args )
        {
            Parser.Default
                .ParseArguments<
                    ConvertCommand, FormatCommand, 
                    InfoCommand>( args )
                .WithParsed<ICommand>( t => t.Execute() );


            //var rootPath = @"C:\_Projects\iHeart.UWP";
            //// parse resource files
            //var resFiles = GetFiles( rootPath, "*.resw" )
            //    .Select( f => new ResourceFile { Path = f } )
            //    .ToList();
            //foreach ( var resFile in resFiles )
            //    resFile.Entires = GetResourceEntries( resFile.Path );

            //// parse *.xaml files
            //var xamlFiles = GetFiles( rootPath, "*.xaml" );
            //var xamlReferences = xamlFiles
            //    .SelectMany( x =>
            //    {
            //        var refs = GetResReferencesXaml( x );

            //        return refs;
            //    } )
            //    .ToList();


            //        break;
            //    case "validate":

            //        foreach ( var resFile in resFiles )
            //        {
            //            Console.ForegroundColor = ConsoleColor.Yellow;
            //            Console.WriteLine( resFile.Path );
            //            Console.ForegroundColor = ConsoleColor.Red;

            //            // get resources with empty values
            //            var emptyEntires = resFile.Entires.Where( e => string.IsNullOrWhiteSpace( e.Value ) ).ToList();
            //            if ( emptyEntires.Any() )
            //            {
            //                Console.WriteLine( $"\tWarning: {emptyEntires.Count} empty resource items." );
            //            }

            //            // get references to missed resources


            //            // get resources without references

            //        }

            //        break;
            //}

        }
    }
}