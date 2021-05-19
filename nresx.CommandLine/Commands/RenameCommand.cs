using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "rename", HelpText = "Rename an element in the resource file(s)" )]
    public class RenameCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key" )]
        public IEnumerable<string> Keys { get; set; }

        //[Option( 'k', "key", HelpText = "element key" )]
        //public IEnumerable<string> Keys { get; set; }



        //[Option( "duplicates", HelpText = "Remove all empty elements - key or value" )]
        //public bool Duplicates { get; set; }

        public override void Execute()
        {
            var sourceFiles = GetSourceFiles();
            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    //var shortFilePath = resource.AbsolutePath.GetShortPath();
                    //var elementsToDelete = new List<string>();

                    //// remove element by key
                    //if ( Keys?.Count() > 0 )
                    //{
                    //    elementsToDelete.AddRange( Keys );
                    //}

                    //// remove all elements with empty key
                    //if ( EmptyKey || EmptyValue || Empty )
                    //{
                    //    elementsToDelete.AddRange( resource
                    //        .Elements
                    //        .Where( element =>
                    //            ( ( EmptyKey || Empty ) && string.IsNullOrWhiteSpace( element.Key ) ) ||
                    //            ( ( EmptyValue || Empty ) && string.IsNullOrWhiteSpace( element.Value ) ) )
                    //        .Select( element => element.Key ) );
                    //}

                    //foreach ( var key in elementsToDelete )
                    //{
                    //    if ( DryRun )
                    //    {
                    //        Console.WriteLine( $"{shortFilePath}: '{key}' have been removed" );
                    //    }
                    //    else
                    //    {
                    //        resource.Elements.Remove( key );
                    //    }
                    //}

                    //if ( !DryRun )
                    //{
                    //    resource.Save( resource.AbsolutePath );
                    //}
                });
        }
    }
}