using System;
using System.Collections.Generic;
using System.Linq;

namespace nresx.Tools.Extensions
{
    public static class ResourceFileExtensions
    {
        public static void ConvertElements( this ResourceFile resourceFile, Action<ResourceElement> convertAction )
        {
            resourceFile.ConvertElements( null, convertAction );
        }
        public static void ConvertElements( 
            this ResourceFile resourceFile, 
            Func<ResourceElement, bool> predicate,
            Action<ResourceElement> convertAction )
        {
            foreach ( var element in resourceFile.Elements )
            {
                if ( predicate != null && !predicate( element ) )
                    continue;
                convertAction( element );
            }
        }
        
        public static void AddPrefix( this ResourceFile resourceFile, string prefix )
        {
            resourceFile.ConvertElements( 
                el => el.Type == ResourceElementType.String && !el.Value.StartsWith( prefix ),
                el => el.Value = $"{prefix}{el.Value}" );
        }

        public static void RemovePrefix( this ResourceFile resourceFile, string prefix )
        {
            resourceFile.ConvertElements(
                el => el.Type == ResourceElementType.String && el.Value.StartsWith( prefix ),
                el => el.Value = el.Value.Substring( prefix.Length ) );
        }

        public static void AddPostfix( this ResourceFile resourceFile, string postfix )
        {
            resourceFile.ConvertElements(
                el => el.Type == ResourceElementType.String && !el.Value.EndsWith( postfix ),
                el => el.Value = $"{el.Value}{postfix}" );
        }
        
        public static void RemovePostfix( this ResourceFile resourceFile, string postfix )
        {
            resourceFile.ConvertElements(
                el => el.Type == ResourceElementType.String && el.Value.EndsWith( postfix ),
                el => el.Value = el.Value.Substring( 0, el.Value.Length - postfix.Length ) );
        }

        public static bool ValidateElements( this ResourceFile resourceFile, out IEnumerable<ResourceElementError> errors )
        {
            var result = new List<ResourceElementError>();
            var keys = new HashSet<string>();
            var keyBases = new HashSet<string>();

            foreach ( var element in resourceFile.Elements )
            {
                // detect duplicates - not applicable, because formatter don't allow to parse duplicates or silently merge it
                //if ( keys.Contains( element.Key ) )
                //{
                //    result.Add( new ResourceElementError( ResourceElementErrorType.Duplicate, element.Key ) );
                //    continue;
                //}
                //else
                //{
                //    keys.Add( element.Key );
                //}

                // detect possible duplicates: i.e. "Key.Content" vs "Key.Text"
                //var baseIndex = element.Key.LastIndexOf( '.' );
                //var keyBase = baseIndex switch
                //{
                //    -1 => element.Key,
                //    0 => element.Key,
                //    > 0 => element.Key.Substring( 0, baseIndex ),
                //    _ => element.Key
                //};
                //if ( !keyBases.Contains( keyBase ) )
                //{
                //    keyBases.Add( keyBase );
                //}
                //else
                //{
                //    result.Add( new ResourceElementError( ResourceElementErrorType.PossibleDuplicate, element.Key ) );
                //}

                // detect empty key
                if ( string.IsNullOrWhiteSpace( element.Key ) )
                {
                    result.Add( new ResourceElementError( 
                        ResourceElementErrorType.EmptyKey, string.Empty, 
                        $"value: {element.Value}" ) );
                }

                // detect empty value
                if ( string.IsNullOrWhiteSpace( element.Value ) )
                {
                    result.Add( new ResourceElementError( ResourceElementErrorType.EmptyValue, element.Key ) );
                }
            }

            errors = result;
            return !errors.Any();
        }
    }


    public enum ResourceElementErrorType
    {
        None = 0x00,
        Duplicate = 0x01,
        PossibleDuplicate = 0x02,
        EmptyKey = 0x03,
        EmptyValue = 0x04,
    }
    public class ResourceElementError
    {
        public ResourceElementErrorType ErrorType { get; }
        public string ElementKey { get; }
        public string Message { get; }

        public ResourceElementError( ResourceElementErrorType errorType, string elementKey, string message = null )
        {
            ErrorType = errorType;
            ElementKey = elementKey;
            Message = message;
        }
    }
}
