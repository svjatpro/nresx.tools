using CommandLine;
using NResx.Tools.CommandLine.Commands;

namespace NResx.Tools.CommandLine
{
    //public class ResourceFile1
    //{
    //    public string Path { get; set; }
    //    public List<ResourceEntry> Entires { get; set; }
    //}
    //public class ResourceEntry
    //{
    //    public string Key { get; set; }
    //    public string Value { get; set; }
    //    public string Comment { get; set; }
    //}

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
        //    Regex resRegex = new ( @"x:Uid\s*=\s*""([a-zA-Z0-9_]+)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

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

        //private static void AddResourcePrefix( string path, string prefix, bool clear = false )
        //{
        //    var doc = XDocument.Load( path );
        //    doc.Root?.Elements( "data" ).ToList().ForEach( el =>
        //    {
        //        var value = el.Element( "value" );
        //        if ( value == null )
        //            return;

        //        if ( clear && value.Value.StartsWith( prefix ) )
        //            value.SetValue( value.Value.Substring( prefix.Length ) );
        //        else
        //            value.SetValue( $"{prefix}{value.Value}" );
        //    } );
        //    doc.Save( path );
        //}


        static void Main( string[] args )
        {
            Parser.Default.ParseArguments<ConvertCommand, InfoCommand>( args )
                .WithParsed<ICommand>( t => t.Execute() );

            //var rootPath = @"C:\_Projects\iHeart.UWP";
            //// parse resource files
            //var resFiles = GetFiles( rootPath, "*.resw" )
            //    .Select( f => new ResourceFile1 { Path = f } )
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


            //var command = args.Length >= 1 ? args[0] : "validate";
            //switch ( command )
            //{
            //    case "add":
            //        if(args.Length > 1 && args[1] == "--clear")
            //            AddResourcePrefix( @"C:\_Projects\iHeart.UWP\IHeartRadio.App\Strings\en\Resources.resw", "en-", clear: true );
            //        else
            //            AddResourcePrefix( @"C:\_Projects\iHeart.UWP\IHeartRadio.App\Strings\en\Resources.resw", "en-" );

            //        break;
            //    case "find":
            //        var target = args[1];

            //        //List<string> result = new();
            //        foreach ( var resFile in resFiles )
            //        {
            //            foreach ( var entry in resFile.Entires )
            //            {
            //                if(entry.Value.Contains( target ))
            //                    Console.WriteLine( $"{resFile.Path} {entry.Key} : {entry.Value}" );
            //            }
            //        }


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