﻿using System;
using CommandLine;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "add", HelpText = "Add new element to the resource file" )]
    public class AddCommand : BaseCommand, ICommand
    {
        [Option( 'k', "key", HelpText = "element key", Required = true )]
        public string Key { get; set; }

        [Option( 'v', "value", HelpText = "element value", Required = true )]
        public string Value { get; set; }

        [Option( 'c', "comment", HelpText = "element comment" )]
        public string Comment { get; set; }

        protected override bool IsCreateNewElementAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;
        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var sourceFiles = GetSourceFiles();
            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    if ( DryRun )
                    {
                        var shortFilePath = resource.AbsolutePath?.GetShortPath() ?? file.FullName.GetShortPath();
                        Console.WriteLine( $"'{Key}: {Value}' element have been add to '{shortFilePath}'" );
                    }
                    else
                    {
                        resource.Elements.Add( Key, Value, Comment );
                        resource.Save( file.FullName );
                    }
                } );
        }
    }
}