using System;
using System.Linq;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "remove", HelpText = "Remove an element from the resource file" )]
    public class RemoveCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key" )]
        public string Key { get; set; }


        [Option( "empty", HelpText = "Remove all empty elements - key or value" )]
        public bool Empty { get; set; }

        [Option( "empty-key", HelpText = "Remove all elements with empty key" )]
        public bool EmptyKey { get; set; }

        [Option( "empty-value", HelpText = "Remove all elements with empty value" )]
        public bool EmptyValue { get; set; }


        //[Option( "duplicates", HelpText = "Remove all empty elements - key or value" )]
        //public bool Duplicates { get; set; }
        
        public void Execute()
        {
            ForEachSourceFile(
                GetSourceFiles(),
                resource =>
                {
                    var shortFilePath = resource.AbsolutePath.GetShortPath();
                    // remove element by key
                    if ( !string.IsNullOrWhiteSpace( Key ) )
                    {
                        if ( DryRun )
                        {
                            Console.WriteLine( $"{shortFilePath}: '{Key}' have been removed" );
                        }
                        else
                        {
                            resource.Elements.Remove( Key );
                        }
                    }

                    // remove all elements with empty key
                    if ( EmptyKey || EmptyValue || Empty )
                    {
                        //foreach ( var element in res.Elements )
                        for ( var i = resource.Elements.Count() - 1; i >= 0; i-- )
                        {
                            var element = resource.Elements[i];
                            if ( ( ( EmptyKey || Empty ) && string.IsNullOrWhiteSpace( element.Key ) ) ||
                                 ( ( EmptyValue || Empty ) && string.IsNullOrWhiteSpace( element.Value ) ) )
                            {
                                if ( DryRun )
                                {
                                    Console.WriteLine( $"{shortFilePath}: '{element.Key}' have been removed" );
                                }
                                else
                                {
                                    resource.Elements.Remove( element.Key );
                                }
                            }
                        }
                    }

                    if( !DryRun )
                        resource.Save( resource.AbsolutePath );
                });
        }
    }
}