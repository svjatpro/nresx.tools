using CommandLine;
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
            ForEachSourceFile(
                GetSourceFiles(),
                ( file, resource ) =>
                {
                    if ( Delete ) // remove
                    {
                        if ( StartWith )
                            resource.RemovePrefix( Pattern );
                        else if ( EndWith )
                            resource.RemovePostfix( Pattern );
                    }
                    else // append
                    {
                        if ( StartWith )
                            resource.AddPrefix( Pattern );
                        else if ( EndWith )
                            resource.AddPostfix( Pattern );
                    }

                    resource.Save( resource.AbsolutePath );
                } );
        }
    }
}