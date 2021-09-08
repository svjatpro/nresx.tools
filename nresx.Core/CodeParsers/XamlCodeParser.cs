using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using nresx.Tools.Extensions;

namespace nresx.Tools.CodeParsers
{
    public class XamlCodeParser : ICodeParser
    {
        private readonly Dictionary<string, int> ElementsCounts = new();

        private readonly Regex ResRegex = new( @"\s(Text|Header|Content|CommandText|OffContent|OnContent|PlaceholderText)\s*=\s*""(.*?)""",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
        private readonly Regex ElementNameRegex = new( @"\s(x:Name|Name)\s*=\s*""(.*?)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
        private readonly Regex ElementTagRegex = new( @"<(.*?)\s+", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

        private bool GetValue( Match match, out string value )
        {
            if ( ( match?.Success ?? false ) &&
                 match.Groups.Count > 2 &&
                 !string.IsNullOrWhiteSpace( match.Groups[2].Value ) &&
                 !match.Groups[2].Value.StartsWith( "{" ) &&
                 !match.Groups[2].Value.EndsWith( "}" ) )
            {
                value = match.Groups[2].Value;
                return true;
            }

            value = null;
            return false;
        }

        private string GenerateElementName( string line, string elementPath, Match match, ref int prevIndex )
        {
            string key = null;
            string keyCore = null;
            string keyExtra = null;
            var prop = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            var valueKey = string.Join( "", value.Split( ' ' ).Take( 2 ).Select( i => i.ToFirstCapital() ) );

            // try to get element name
            var nameMatch = ElementNameRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
            if ( nameMatch.Success )
            {
                keyCore = $"{elementPath}_{nameMatch.Groups[2].Value.ToFirstCapital()}";
                keyExtra = $".{prop}";
                key = $"{keyCore}{keyExtra}";
                prevIndex = match.Length + match.Index + 1;
            }

            // try to get tag name
            if ( key == null )
            {
                var tagMatch = ElementTagRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
                if ( tagMatch.Success )
                {
                    var tag = tagMatch.Groups[1].Value.ToFirstCapital();
                    keyCore = $"{elementPath}_{tagMatch.Groups[1].Value.ToFirstCapital()}";
                    keyExtra = $"_{valueKey}.{prop}";
                    key = $"{keyCore}{keyExtra}";
                    prevIndex = match.Length + match.Index + 1;
                }
            }

            // get name from value
            if ( key == null )
            {
                keyCore = $"{elementPath}_{valueKey}";
                keyExtra = $".{prop}";
                key = $"{keyCore}{keyExtra}";
            }

            // check element name duplicates
            var keyIndex = key.ToLower();
            if ( ElementsCounts.ContainsKey( keyIndex ) )
            {
                key = $"{keyCore}{ElementsCounts[keyIndex]++}{keyExtra}";
            }
            else
            {
                ElementsCounts.Add( keyIndex, 1 );
            }

            return key;
        }

        public Dictionary<string, string> ParseLine( string line, string elementPath )
        {
            var result = new Dictionary<string, string>();

            var prevIndex = 0;
            var matches = ResRegex.Matches( line );
            foreach ( Match match in matches )
            {
                if ( !GetValue( match, out var value ) ) continue;
                var key = GenerateElementName( line, elementPath, match, ref prevIndex );
                result.Add( key, value );
            }

            return result;
        }
    }
}