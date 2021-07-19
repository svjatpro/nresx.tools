using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using nresx.CommandLine.Commands.Base;
using nresx.Tools;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands
{
    [Verb( "validate", HelpText = "Validate resource file(s)" )]
    public class ValidateCommand : BaseCommand
    {
        protected override bool IsRecursiveAllowed => true;

        public override void Execute()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
            if ( !optionsParsed )
                return;

            ForEachResourceGroup( sourceFiles, (context, group) =>
            {
                var resourceMap = new Dictionary<string, Dictionary<int, string>>();
                group.ForEach( resource =>
                {
                    foreach ( var element in resource.Elements )
                    {
                        resourceMap.TryAdd( element.Key, new Dictionary<int, string>() );
                        resourceMap[element.Key].TryAdd( resource.GetHashCode(), element.Value );
                    }
                } );

                group.ForEach( resource =>
                {
                    var elements = ResourceFile.LoadRawElements( resource.AbsolutePath );
                    var result = elements.ValidateElements( out var errors );

                    // validate missed translations 
                    var missed = resourceMap.Keys.Except( resource.Elements.Select( el => el.Key ) ).ToList();
                    if ( missed.Any() )
                    {
                        result = false;
                        errors.AddRange( missed.Select( el => new ResourceElementError( ResourceElementErrorType.MissedElement, el ) ) );
                    }

                    // validate not translated elements
                    foreach ( var el in resource.Elements )
                    {
                        var elMap = resourceMap[el.Key];
                        if ( elMap.Any( r => r.Key != resource.GetHashCode() && r.Value == el.Value ) )
                        {
                            result = false;
                            errors.Add( new ResourceElementError( ResourceElementErrorType.NotTranslated, el.Key ) );
                        }
                    }

                    if ( result )
                    {
                        //
                    }
                    else
                    {
                        if ( context.TotalResourceFiles > 1 && errors.Any() )
                        {
                            Console.WriteLine( $"Resource file: \"{resource.AbsolutePath}\"" );
                        }

                        foreach ( var elementError in errors )
                        {
                            var msg = new StringBuilder();
                            msg.Append( $"{elementError.ErrorType}:" );
                            if ( !string.IsNullOrWhiteSpace( elementError.ElementKey ) )
                                msg.Append( $" {elementError.ElementKey};" );
                            if ( !string.IsNullOrWhiteSpace( elementError.Message ) )
                                msg.Append( $" {elementError.Message};" );
                            Console.WriteLine( msg.ToString() );
                        }
                    }
                } );
            } );
        }
    }
}