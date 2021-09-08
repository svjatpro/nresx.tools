using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using nresx.Tools.Extensions;

namespace nresx.Tools.CodeParsers
{
    public class CsCodeParser : ICodeParser
    {
        private readonly Dictionary<string, int> ElementsCounts = new();

        private readonly Regex ElementRegex = new( @"""(.*?)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
        private readonly Regex AsmRegex = new( @"\[assembly:", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
        private readonly Regex CommentRegex = new( @"^\s*///", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
        private readonly Regex ThrowRegex = new( @"\s+throw new ", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );

        private bool ValidateLine( string line )
        {
            if ( AsmRegex.IsMatch( line ) ||
                 CommentRegex.IsMatch( line ) ||
                 ThrowRegex.IsMatch( line ) )
            {
                return false;
            }

            return true;
        }
        private bool GetValue( Match match, out string value )
        {
            if ( ( match?.Success ?? false ) &&
                 match.Groups.Count >= 1 &&
                 !string.IsNullOrWhiteSpace( match.Groups[1].Value ) )
            {
                value = match.Groups[1].Value;
                return true;
            }

            value = null;
            return false;
        }

        private string GenerateElementName( string line, string elementPath, Match match, ref int prevIndex )
        {
            string key = null;
            var value = match.Groups[1].Value;

            // try to get local variable name
            Regex csVarRegex = new( $@"[$\s;]+([a-zA-Z0-9_]+)\s*=\s*""{value}""",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
            var varMatch = csVarRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
            if ( varMatch.Success )
            {
                key = $"{elementPath}_{varMatch.Groups[1].Value.ToFirstCapital()}";
                prevIndex = match.Length + match.Index + 1;
            }

            // try to get property name
            if ( key == null )
            {
                Regex csPropRegex = new( $@"[$\s;]+([a-zA-Z0-9_]+)\s*=>\s*""{value}""",
                    RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
                var propMatch = csPropRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
                if ( propMatch.Success )
                {
                    key = $"{elementPath}_{propMatch.Groups[1].Value.ToFirstCapital()}";
                    prevIndex = match.Length + match.Index + 1;
                }
            }

            // get name from value
            if ( key == null )
            {
                key = $"{elementPath}_{string.Join( "", value.Split( ' ' ).Take( 2 ).Select( i => i.ToFirstCapital() ) )}";
            }

            return key;
        }

        public Dictionary<string, string> ParseLine( string line, string elementPath )
        {
            var result = new Dictionary<string, string>();

            if ( !ValidateLine( line ) )
                return result;

            var prevIndex = 0;
            var matches = ElementRegex.Matches( line );
            foreach ( Match match in matches )
            {
                if ( !GetValue( match, out var value ) ) continue;
                var key = GenerateElementName( line, elementPath, match, ref prevIndex );

                // check element name duplicates
                var keyIndex = key.ToLower();
                if ( ElementsCounts.ContainsKey( keyIndex ) )
                {
                    key = $"{key}{ElementsCounts[keyIndex]++}";
                }
                else
                {
                    ElementsCounts.Add( keyIndex, 1 );
                }

                result.Add( key, value );
            }

            return result;
        }
    }
}
