using System;
using System.Linq;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb("format", HelpText = "format resource texts")]
    public class FormatCommand : BaseCommand, ICommand
    {
        [Option( 'p', "pattern", HelpText = "Pattern to apply to all texts", Required = false )]
        public string Pattern { get; set; }

        [Option( "start-with", HelpText = "Add or remove prefix to all texts", Required = false )]
        public bool StartWith { get; set; }

        [Option( "end-with", HelpText = "Add or remove postfix to all texts", Required = false )]
        public bool EndWith { get; set; }

        [Option( 'l', "language-code", HelpText = "Use language code as prefix or postfix", Required = false )]
        public bool LanguageCode { get; set; }

        [Option( 'c', "culture-code", HelpText = "Use culture code as prefix or postfix", Required = false )]
        public bool CultureCode { get; set; }

        [Option( "delete", HelpText = "Remove prefix or postfix", Required = false)]
        public bool Delete { get; set; }


        protected override bool IsCreateNewElementAllowed => true;
        protected override bool IsCreateNewFileAllowed => true;
        protected override bool IsFormatAllowed => true;
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
            if ( !optionsParsed )
                return;
            
            ForEachSourceFile(
                sourceFiles,
                ( file, resource ) =>
                {
                    var pattern = Pattern;
                    if ( LanguageCode || CultureCode )
                    {
                        resource.AbsolutePath.TryToExtractCultureFromPath( out var culture );
                        var code = CultureCode ? culture.Name : culture.TwoLetterISOLanguageName;
                        if ( StartWith )
                            pattern = $"{code}_";
                        else if( EndWith )
                            pattern = $"_{code}";
                    }

                    if ( Delete ) // remove
                    {
                        if ( StartWith )
                        {
                            resource.RemovePrefix( pattern );
                        }
                        else if ( EndWith )
                        {
                            resource.RemovePostfix( pattern );
                        }
                    }
                    else // append
                    {
                        if ( StartWith )
                        {
                            resource.AddPrefix( pattern );
                        }
                        else if ( EndWith )
                        {
                            resource.AddPostfix( pattern );
                        }
                    }

                    Console.WriteLine( $"{resource.Elements.Count()} elements have been formatted in '{resource.AbsolutePath.GetShortPath()}'" );
                    if ( !DryRun )
                    {
                        resource.Save( resource.AbsolutePath );
                    }
                } );
        }
    }
}