using System;
using System.Collections.Generic;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "copy", HelpText = "Copy elements from one file to another" )]
    public class CopyCommand : BaseCommand, ICommand
    {
        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public IEnumerable<string> DestinationFiles { get; set; }

        [Option( "skip", HelpText = "Skip duplicated elements" )]
        public bool SkipDuplicates { get; set; }

        [Option( "overwrite", HelpText = "Overwrite duplicated elements" )]
        public bool OverwriteDuplicates { get; set; }
        
        protected override bool IsRecursiveAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;
        protected override bool IsFormatAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true )
                .Single( DestinationFiles, out var destFile, mandatory: true )
                .Validate();

            if ( !optionsParsed )
                return;

            var destination = new ResourceFile( destFile );
            if ( destination.IsNewFile && !CreateNewFile )
            {
                Console.WriteLine( FilesNotFoundErrorMessage, destFile.GetShortPath() );
                return;
            }

            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    foreach ( var element in resource.Elements )
                    {
                        var destElement = destination.Elements[element.Key];
                        var src = (value : element?.Value ?? string.Empty, comment : element?.Comment ?? string.Empty);
                        var dest = (value : destElement?.Value ?? string.Empty, comment : destElement?.Comment ?? string.Empty);

                        if ( destElement == null )
                        {
                            if ( !DryRun )
                                destination.Elements.Add( element.Key, src.value, src.comment );
                            Console.WriteLine( $"'{element.Key}' element have been copied to '{destFile.GetShortPath()}' file" );
                        }
                        else if ( OverwriteDuplicates && ( src.value != dest.value || src.comment != dest.comment ) )
                        {
                            if ( !DryRun )
                            {
                                destElement.Value = src.value;
                                destElement.Comment = src.comment;
                            }
                            Console.WriteLine( $"'{element.Key}' element have been overwritten in '{destFile.GetShortPath()}' file" );
                        }
                    }
                } );

            if ( !DryRun )
            {
                destination.Save( destFile );
            }
        }
    }
}