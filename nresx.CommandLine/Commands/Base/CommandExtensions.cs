using System;
using System.Collections.Generic;
using System.Linq;
using nresx.Tools.Extensions;

namespace nresx.CommandLine.Commands.Base
{
    public static class CommandExtensions
    {
        private const string MissingOptionMessage = "Required option '{0}' is missing.";

        public static OptionContext Multiple( 
            this OptionContext context, 
            IEnumerable<string> mappedValues, 
            out List<string> values, 
            bool mandatory = false, 
            string optionName = null )
        {
            var src = mappedValues?.ToList();
            var args = context.FreeArgs?.ToList();
            var errors = context.Errors;
            var result = new List<string>();

            if ( src?.Count > 0 )
            {
                result.AddRange( src );
            }
            else if( args.TryTake( out var val ) )
            {
                result.Add( val );
            }
            values = result.Distinct().ToList();

            var success = !mandatory || values.Count > 0;
            if ( !success && optionName != null )
                errors.Add( string.Format( MissingOptionMessage, optionName ) );

            return new OptionContext( args, success, errors );
        }

        public static OptionContext Single(
            this OptionContext context,
            string mappedValue,
            out string value,
            bool mandatory = false,
            string optionName = null )
        {
            return context.Single( new[] {mappedValue}, out value, mandatory, optionName );
        }
        public static OptionContext Single( 
            this OptionContext context,
            IEnumerable<string> mappedValues,
            out string value, 
            bool mandatory = false,
            string optionName = null )
        {
            var args = context.FreeArgs?.ToList();
            var errors = context.Errors;

            var first = mappedValues?.FirstOrDefault();
            if ( !string.IsNullOrWhiteSpace( first ) )
            {
                value = first;
            }
            else if ( args.TryTake( out var val ) )
            {
                value = val;
            }
            else
            {
                value = null;
            }

            var success = !mandatory || !string.IsNullOrWhiteSpace( value );
            if ( !success && optionName != null )
                errors.Add( string.Format( MissingOptionMessage, optionName ) );

            return new OptionContext( args, success, errors );
        }

        public static bool Validate( this OptionContext context )
        {
            if ( !context.Success )
            {
                foreach ( var error in context.Errors )
                {
                    Console.WriteLine( error );
                }
            }
            return context.Success;
        }
    }
}