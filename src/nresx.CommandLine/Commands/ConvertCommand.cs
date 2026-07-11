using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Core;
using nresx.Core.Extensions;
using nresx.Core.Helpers;

namespace nresx.CommandLine.Commands
{
    [Verb("convert", HelpText = "Convert to another format")]
    public class ConvertCommand : BaseCommand, ICommand
    {
        protected override bool IsCreateNewElementAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;
        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;

        [Option( 'd', "destination", HelpText = "Destination resource file" )]
        public IEnumerable<string> DestinationFiles { get; set; }

        protected override IEnumerable<string> HelpExamples =>
        [
            "# will convert single resource file (res1.resx) to .po format and save with new name (res2.po)\n" +
            "nresx convert path1/res1.resx path2/res2.po",
            "# will convert single resource file (res1.resx) to .yaml format and save as (res1.yaml) in the same folder\n" +
            "nresx convert res1.resx -f yaml",
            "# will convert all resource files in current folder and all subdirectories\n" +
            "#   to .yaml format and save with the same name (but with .yaml extension) in an appropriate folder\n" +
            "nresx convert *.resx -f yaml --recursive",
        ];

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, optionName: "source" )
                .Multiple( DestinationFiles, out var destFiles, mandatory: false )
                .Validate( this );
            if ( !optionsParsed )
                return;

            var optionFormat = ResourceFormatType.NA;
            string optionExtension = null;
            if ( !string.IsNullOrWhiteSpace( Format ) &&
                 OptionHelper.DetectResourceFormat( Format, out var f1 ) )
            {
                optionFormat = f1;
            }

            if ( destFiles?.Count == 0 )
            {
                if ( optionFormat != ResourceFormatType.NA &&
                     ResourceFormatHelper.DetectExtension( optionFormat, out var ext ) )
                {
                    optionExtension = ext;
                }
                else
                {
                    WriteError( ExitFormatError, FormatUndefinedErrorMessage );
                    return;
                }
            }

            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    var destinations = destFiles ?? new List<string>();
                    if ( !destinations.Any() )
                    {
                        if ( !string.IsNullOrWhiteSpace( optionExtension ) )
                        {
                            destinations = new List<string> 
                            {
                                Path.ChangeExtension( resource.AbsolutePath, optionExtension )
                            };
                        }
                        else
                        {
                            WriteError( ExitFormatError, FormatUndefinedErrorMessage ); // never happen?
                            return;
                        }
                    }
                    
                    foreach ( var dest in destinations )
                    {
                        var destination = dest;
                        ResourceFormatType format;
                        if ( !string.IsNullOrWhiteSpace( destination ) &&
                             ResourceFormatHelper.DetectFormatByExtension( destination, out var f ) )
                        {
                            format = f;
                        }
                        else if ( optionFormat != ResourceFormatType.NA )
                        {
                            format = optionFormat;
                        }
                        else
                        {
                            WriteError( ExitFormatError, FormatUndefinedErrorMessage );
                            return;
                        }

                        if ( ResourceFormatHelper.DetectExtension( format, out var extension ) )
                            destination = Path.ChangeExtension( destination, extension );
                        // else throw

                        if ( !destination.IsRegularName() )
                        {
                            // try to extract destination path
                            var destPath = Path.GetDirectoryName( destination );
                            if ( destPath.IsRegularName() )
                            {
                                var destInfo = new DirectoryInfo( destPath );
                                if ( !destInfo.Exists )
                                    destInfo.Create();

                                destination = Path.ChangeExtension( Path.Combine( destPath, resource.FileName ), extension );
                            }
                            else
                            {
                                destination = Path.ChangeExtension( resource.AbsolutePath, extension );
                            }
                        }

                        var destFile = new FileInfo( destination );
                        if ( resource.AbsolutePath == destFile.FullName ) // the same name
                        {
                            WriteError( ExitDestinationConflict, FileAlreadyExistErrorMessage, destination.GetShortPath() );
                            return;
                        }
                        if ( destFile.Exists /* overwrite option */ )
                        {
                            WriteError( ExitDestinationConflict, FileAlreadyExistErrorMessage, destination.GetShortPath() );
                            return;
                        }

                        WriteVerbose( "writing: {0} (format: {1}, {2} elements)", destination, format, resource.Elements.Count() );
                        Console.WriteLine( $"'{resource.AbsolutePath.GetShortPath()}' resource have been converted to '{destination.GetShortPath()}'" );

                        if ( !DryRun )
                        {
                            resource.Save( destination, format );
                        }
                    }
                } );
        }
    }
}