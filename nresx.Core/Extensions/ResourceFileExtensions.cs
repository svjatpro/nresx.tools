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

        public static bool ValidateElements( this IEnumerable<ResourceElement> elements, out List<ResourceElementError> errors )
        {
            var result = new List<ResourceElementError>();
            var keys = new HashSet<string>();
            var keyBases = new HashSet<string>();

            foreach ( var element in elements )
            {
                // detect duplicates
                if ( keys.Contains( element.Key ) )
                {
                    result.Add( new ResourceElementError( ResourceElementErrorType.Duplicate, element.Key ) );
                    continue;
                }
                else
                {
                    keys.Add( element.Key );
                }

                // detect possible duplicates: i.e. "Key.Content" vs "Key.Text"
                var baseIndex = element.Key.LastIndexOf( '.' );
                var keyBase = baseIndex switch
                {
                    -1 => element.Key,
                    0 => element.Key,
                    > 0 => element.Key.Substring( 0, baseIndex ),
                    _ => element.Key
                };
                if ( !keyBases.Contains( keyBase ) )
                {
                    keyBases.Add( keyBase );
                }
                else
                {
                    result.Add( new ResourceElementError( ResourceElementErrorType.PossibleDuplicate, element.Key ) );
                }

                // detect empty key
                if ( string.IsNullOrWhiteSpace( element.Key ) )
                {
                    result.Add( new ResourceElementError(
                        ResourceElementErrorType.EmptyKey, "",
                        $"(value: {element.Value})" ) );
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


    /// <summary>Categorizes a single validation finding raised against a <see cref="ResourceElement"/>.</summary>
    public enum ResourceElementErrorType
    {
        /// <summary>Unspecified.</summary>
        None = 0x00,
        /// <summary>Two or more elements share the same key (file is broken).</summary>
        Duplicate = 0x01,
        /// <summary>Heuristic match: keys differ only in case or whitespace.</summary>
        PossibleDuplicate = 0x02,
        /// <summary>Element has no key (file is broken).</summary>
        EmptyKey = 0x03,
        /// <summary>Element has a key but no value.</summary>
        EmptyValue = 0x04,

        /// <summary>Element is present in the base file but missing from a translation file.</summary>
        MissedElement = 0x05,
        /// <summary>Translation file's value equals the base file's value — the element wasn't translated.</summary>
        NotTranslated = 0x06,
    }

    /// <summary>Severity bucket for a <see cref="ResourceElementErrorType"/>. Drives <c>nresx validate</c>'s exit code (RSX-229).</summary>
    public enum ResourceElementErrorSeverity
    {
        /// <summary>Non-fatal: quality/heuristic finding. Validate exits 0 unless <c>--warnings-as-errors</c> is set.</summary>
        Warning = 0,
        /// <summary>Fatal: file would fail at runtime. Validate exits non-zero.</summary>
        Error = 1,
    }

    /// <summary>Extension that maps each <see cref="ResourceElementErrorType"/> to its <see cref="ResourceElementErrorSeverity"/>.</summary>
    public static class ResourceElementErrorTypeExtensions
    {
        /// <summary>
        /// Maps an error type to its severity per the RSX-229 policy:
        /// <list type="bullet">
        /// <item><description><c>Error</c> = file is broken / will fail at runtime (duplicate keys, empty keys)</description></item>
        /// <item><description><c>Warning</c> = quality issue / heuristic / common in-progress state (everything else)</description></item>
        /// </list>
        /// </summary>
        public static ResourceElementErrorSeverity GetSeverity( this ResourceElementErrorType errorType )
        {
            return errorType switch
            {
                ResourceElementErrorType.Duplicate         => ResourceElementErrorSeverity.Error,
                ResourceElementErrorType.EmptyKey          => ResourceElementErrorSeverity.Error,
                ResourceElementErrorType.EmptyValue        => ResourceElementErrorSeverity.Warning,
                ResourceElementErrorType.PossibleDuplicate => ResourceElementErrorSeverity.Warning,
                ResourceElementErrorType.MissedElement     => ResourceElementErrorSeverity.Warning,
                ResourceElementErrorType.NotTranslated     => ResourceElementErrorSeverity.Warning,
                _                                          => ResourceElementErrorSeverity.Warning,
            };
        }
    }

    /// <summary>A single validation finding produced by <c>Validate</c> extension methods.</summary>
    public class ResourceElementError
    {
        /// <summary>The kind of validation issue.</summary>
        public ResourceElementErrorType ErrorType { get; }
        /// <summary>Key of the element the error is attached to (may be empty for file-level findings).</summary>
        public string ElementKey { get; }
        /// <summary>Human-readable detail. May be empty/null when <see cref="ErrorType"/> alone is enough.</summary>
        public string Message { get; }

        /// <summary>Creates a new validation finding.</summary>
        public ResourceElementError( ResourceElementErrorType errorType, string elementKey, string message = null )
        {
            ErrorType = errorType;
            ElementKey = elementKey;
            Message = message;
        }
    }
}
