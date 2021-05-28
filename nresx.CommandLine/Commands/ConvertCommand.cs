using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.Extensions;
using nresx.Tools.Helpers;

namespace nresx.CommandLine.Commands
{
    [Verb("convert", HelpText = "Convert to another format")]
    public class ConvertCommand : BaseCommand, ICommand
    {
        protected override bool IsCreateNewElementAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;
        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true )
                .Multiple( DestinationFiles, out var destFiles, mandatory: false )
                .Validate();
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
                    Console.WriteLine( FormatUndefinedErrorMessage );
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
                            Console.WriteLine( FormatUndefinedErrorMessage ); // never happen?
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
                            Console.WriteLine( FormatUndefinedErrorMessage );
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
                            Console.WriteLine( FileAlreadyExistErrorMessage, destination.GetShortPath() );
                            return;
                        }
                        if ( destFile.Exists /* overwrite option */ )
                        {
                            Console.WriteLine( FileAlreadyExistErrorMessage, destination.GetShortPath() );
                            return;
                        }

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