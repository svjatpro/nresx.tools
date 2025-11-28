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

        protected override void ExecuteCommand()
        {
            var optionsParsed = Options()
                .Multiple( SourceFiles, out var sourceFiles, mandatory: true, multipleIndirect: true )
                .Validate();
            if ( !optionsParsed )
                return;

            ForEachResourceGroup( sourceFiles, (context, group) =>
            {
                var resourceMap = new Dictionary<string, Dictionary<int, string>>();
                var resources = group
                    .Select( f =>
                    {
                        var elements = ResourceFile.LoadRawElements( f.FileInfo.FullName ).ToList();
                        foreach ( var element in elements )
                        {
                            resourceMap.TryAdd( element.Key ?? string.Empty, new Dictionary<int, string>() );
                            resourceMap[element.Key ?? string.Empty].TryAdd( f.FileInfo.GetHashCode(), element.Value );
                        }

                        return ( f.FileInfo, elements );
                    } )
                    .ToList();

                resources.ForEach( f =>
                {
                    //var elements = ResourceFile.LoadRawElements( f.FileInfo.FullName );
                    var result = f.elements.ValidateElements( out var errors );

                    // validate missed translations 
                    var missed = resourceMap.Keys.Except( f.elements.Select( el => el.Key ) ).ToList();
                    if ( missed.Any() )
                    {
                        result = false;
                        errors.AddRange( missed.Select( el => new ResourceElementError( ResourceElementErrorType.MissedElement, el ) ) );
                    }

                    // validate not translated elements
                    foreach ( var el in f.elements )
                    {
                        var elMap = resourceMap[el.Key];
                        if ( elMap.Any( r => r.Key != f.FileInfo.GetHashCode() && r.Value == el.Value ) )
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
                            Console.WriteLine( $"Resource file: \"{f.FileInfo.FullName}\"" );
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