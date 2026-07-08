using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using nresx.CommandLine.Commands;
using nresx.CommandLine.Commands.Base;

namespace nresx.CommandLine;

public class CommandLineContext
{
    public readonly Type[] CommandTypes =
    [
        typeof(VersionCommand), typeof(HelpCommand),
        typeof(InfoCommand), typeof(ListCommand),
        typeof(ConvertCommand), typeof(FormatCommand), typeof(CopyCommand),
        typeof(AddCommand), typeof(RemoveCommand), typeof(UpdateCommand), typeof(RenameCommand),
        typeof(ValidateCommand), typeof(GenerateCommand)
    ];
}

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

    static int Main( string[] args )
    {
        //var res1 = new ResourceFile( @"C:\Tmp\2\WebApp.po" );
        //res1.Save( @"C:\Tmp\2\WebApp1.po" );
        //return 0;

        //using var stream = new FileStream( @"C:\Tmp\Resources.resw", FileMode.Open, FileAccess.Read, FileShare.ReadWrite );
        //using var reader = new ResXResourceReader( stream );
        //reader.UseResXDataNodes = true;
        //var result = new List<ResourceElement>();
        //foreach ( DictionaryEntry item in reader )
        //{
        //    var node = item.Value as ResXDataNode;
        //    var nodeInfo = node?.GetDataNodeInfo();
        //    result.Add( new ResourceElement
        //    {
        //        Type = ResourceElementType.String, // 
        //        Key = item.Key.ToString(),
        //        Value = nodeInfo?.ValueData ?? item.Value.ToString(),
        //        Comment = nodeInfo?.Comment
        //    } );
        //}
        //var doc = XDocument.Load( @"C:\Tmp\Resources.resw" );
        //var res1 = new ResourceFile( @"C:\Tmp\Resources.resw" );

        var infoCommand = ( typeof(InfoCommand).GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )?
            .Name?.ToLower() ?? "info";
        var versionCommand = ( typeof(VersionCommand).GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )?
            .Name?.ToLower() ?? "version";
        var helpCommand = ( typeof(HelpCommand).GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )?
            .Name?.ToLower() ?? "help";

        var commandInterface = typeof(ICommand);
        var commands = commandInterface.Assembly
            .GetTypes().Where( t => t.IsClass && !t.IsAbstract && t.IsAssignableTo( commandInterface ) )
            .Select( t => t.GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )
            .Where( a => a != null )
            .Select( a => a.Name.ToLower() )
            .Select( cmd => (key: cmd, command: cmd));

        var commandMap =
            new[]
                {
                    (key: "-v", command: versionCommand),
                    (key: "--version", command: versionCommand),
                    (key: "-h", command: helpCommand),
                    (key: "--help", command: helpCommand),
                }
                .Concat( commands )
                .ToDictionary( cmd => cmd.key, cmd => cmd.command, StringComparer.CurrentCultureIgnoreCase );
        var parsedCommand = (args.Any() && commandMap.TryGetValue( args[0].ToLower(), out var c )) ? c : null;

        var parser = new Parser(s =>
        {
            s.AutoVersion = false;
            s.AutoHelp = false;
        });
        var context = new CommandLineContext();

        // `nresx <command> --help` / `-h`: render detailed help for that command and exit 0
        // (help is a successful outcome, not a usage error). `-h`/`--help` as the first arg
        // still routes to the general command list via commandMap above.
        if ( parsedCommand != null && parsedCommand != helpCommand && parsedCommand != versionCommand &&
             args.Skip( 1 ).Any( a => a is "-h" or "--help" ) )
        {
            var cmdType = context.CommandTypes.FirstOrDefault( t =>
                string.Equals(
                    ( t.GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )?.Name,
                    parsedCommand, StringComparison.CurrentCultureIgnoreCase ) );
            if ( cmdType != null )
            {
                HelpRenderer.Render( cmdType );
                return BaseCommand.ExitSuccess;
            }
        }

        //if ( parsedCommand == versionCommand )
        //{
        //    Console.WriteLine( $@"nresx version: {ResourceManager.GetVersion()}" );
        //    return 0;
        //}
        //if( parsedCommand == helpCommand || args.Length == 0 )
        //{
        //    Console.WriteLine(@"nresx help");
        //    var help = HelpText.AutoBuild(
        //        parser.ParseArguments(args, commandTypes),
        //        h =>
        //        {
        //            h.Heading = "nresx - resource tool";
        //            h.Copyright = "© 2025 nresx";
        //            //h.Version = "0.3.0";
        //            return h;
        //        },
        //        e => e);
        //    Console.WriteLine(help);
        //    return 0;
        //}

        var arguments = args.Length switch
        {
            0 => [helpCommand],
            1 when parsedCommand == null => [infoCommand, args[0]], // info <filename>
            _ when parsedCommand == versionCommand => [versionCommand],
            _ when parsedCommand == helpCommand => [helpCommand,..args[1..]],
            _ when parsedCommand == null => [infoCommand,..args],
            _ => args
        };

        //return Parser.Default
        var parsedContext = parser.ParseArguments(arguments, context.CommandTypes);
        return parsedContext
            //new Parser(settings =>
            //{
            //    settings.AutoVersion = false;
            //    settings.AutoHelp = false;
            //})
            //.ParseArguments<
            //    VersionCommand, HelpCommand,
            //    InfoCommand, ListCommand,
            //    ConvertCommand, FormatCommand, CopyCommand,
            //    AddCommand, RemoveCommand, UpdateCommand, RenameCommand,
            //    ValidateCommand, GenerateCommand>( arguments )
            //.ParseArguments(arguments, commandTypes)
            .WithParsed<ICommand>( t =>
            {
                t.SetContext( context );
                t.Execute();
            })
            .WithNotParsed( errors =>
            {
                var cmd = new HelpCommand();
                cmd.SetContext( context );
                cmd.Execute();
                //Console.Error.WriteLine("Invalid command line arguments.");
                //foreach ( var err in errors ) Console.Error.WriteLine( $"\t{err}" );
            })
            .MapResult(
                cmd => ((ICommand)cmd).ExitCode,
                err => BaseCommand.ExitUsageError );


        // ---------------------------------------------------------------------------
        /*
        // find references
        var res1 = new ResourceFile( @"C:\_Projects\iHeart.UWP.Localize\IHeartRadio.App\Strings\en\Resources.resw" );
        var elements = res1
            .Elements.Select( el => (key: el.Key, references: new List<string>() ) )
            .ToList();

        FilesHelper.SearchFiles( @"C:\_Projects\iHeart.UWP.Localize\IHeartRadio.App\*.cs", file =>
        {
            if ( file.FullName.ContainsDir( "bin", "obj" ) )
                return;

            using var reader = new StreamReader( new FileStream( file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );

            var fileBody = reader.ReadToEnd();

            foreach ( var el in elements )
            {
                if( fileBody.Contains( $"\"{el.key}\"" ) )
                    el.references.Add( file.FullName );
            }
        }, recursive: true );

        FilesHelper.SearchFiles( @"C:\_Projects\iHeart.UWP.Localize\IHeartRadio.App\*.xaml", file =>
        {
            if ( file.FullName.ContainsDir( "bin", "obj" ) )
                return;

            using var reader = new StreamReader( new FileStream( file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );

            var fileBody = reader.ReadToEnd();

            foreach ( var el in elements )
            {
                var elKey = el.key.Split( '.' ).First();
                if ( fileBody.Contains( $"\"{elKey}\"" ) )
                    el.references.Add( file.FullName );
            }
        }, recursive: true );

        var emptyRefs = elements.Where( el => !el.references.Any() ).ToList();

        foreach ( var el in emptyRefs )
        {
            Console.WriteLine(el.key);
        }

        return 0;

        // ---------------------------------------------------------------------------

        FilesHelper.SearchFiles( "*.xaml",
            file =>
            {
                if ( file.FullName.ContainsDir( "bin", "obj" ) )
                    return;


                //x:Uid="SettingsPage_Title"
                //Regex resRegex = new( @"x:Uid\s*=\s*""([a-zA-Z0-9_]+)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
                //Regex resRegex = new( @"\sText|Header|Content|CommandText|OffContent|OnContent|PlaceholderText\s*=\s*""([a-zA-Z0-9_]*)""",
                //Regex resRegex = new( @"\sText|Header|Content|CommandText|OffContent|OnContent|PlaceholderText\s*=\s*""([\w\W^""]*)""",
                Regex resRegex = new( @"\s(Text|Header|Content|CommandText|OffContent|OnContent|PlaceholderText)\s*=\s*""([\w\W^""]*)""",
                    RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

                    //| Content | CommandText | OffContent | OnContent | PlaceholderText
                // Header Content CommandText OffContent OnContent PlaceholderText

                using var reader = new StreamReader( new FileStream( file.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite ) );
                var refs = new List<string>();
                while ( !reader.EndOfStream )
                {
                    var line = reader.ReadLine();
                    if ( string.IsNullOrWhiteSpace( line ) )
                        continue;
                    var match = resRegex.Match( line );
                    if ( match.Success &&
                         match.Groups.Count > 2 &&
                         !string.IsNullOrWhiteSpace( match.Groups[2].Value ) &&
                         !match.Groups[2].Value.StartsWith( '{' ) &&
                         !match.Groups[2].Value.EndsWith( '}' ) )
                    {
                        Console.WriteLine( $"{file.Name}: {line}" );

                        //refs.AddRange( match.Groups.Values.Skip( 1 ).Select( g => g.Value ) );
                        //refs.Add( await reader.ReadLineAsync() );
                    }
                }


                //Console.WriteLine( file.FullName );
            },
            ( file, ex ) =>
            {

            },
            recursive: true );


         return 0;
        */

        // ---------------------------------------------------------------------------

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