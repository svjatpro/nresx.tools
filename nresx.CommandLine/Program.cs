﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CommandLine;
using nresx.CommandLine.Commands;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

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

        static int Main( string[] args )
        {
            var arguments = args;

            var infoCommand = ( typeof(InfoCommand).GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )?.Name ?? "info";
            var commandInterface = typeof(ICommand);
            var commands = commandInterface.Assembly
                .GetTypes().Where( t => t.IsClass && !t.IsAbstract && t.IsAssignableTo( commandInterface ) )
                .Select( t => t.GetCustomAttribute( typeof(VerbAttribute) ) as VerbAttribute )
                .Where( a => a != null )
                .Select( a => a.Name )
                .ToList();
            
            if ( !args.Any() || !commands.Contains( args[0] ) )
            {
                switch ( args.Length )
                {
                    // if no args, then run info command for current dir with recursive option
                    case 0:
                        arguments = new[] { infoCommand };
                        break;
                    // single argument without command is considered as "info <filename>"
                    case 1:
                        // todo: check is it file or dir
                        arguments = new[] { infoCommand, args[0] };
                        break;
                    // multiple arguments without command are considered as "convert -s <source file1> <source file2> ..."
                    default:
                        arguments = new string[args.Length + 2];
                        arguments[0] = infoCommand;
                        arguments[1] = "-s";
                        args.CopyTo( arguments, 2 );
                        break;
                }
            }

            return Parser.Default
                .ParseArguments<
                    InfoCommand, ListCommand,
                    ConvertCommand, FormatCommand, CopyCommand,
                    AddCommand, RemoveCommand, UpdateCommand, RenameCommand,
                    ValidateCommand>( arguments )
                .WithParsed<ICommand>( t => t.Execute() )
                .MapResult( 
                    cmd => ((ICommand)cmd).Successful ? 0 : -1,
                    err => -1 );

            /*FilesHelper.SearchFiles( "*.xaml",
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
            */

            // return 0;

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