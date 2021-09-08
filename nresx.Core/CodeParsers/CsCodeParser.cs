using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using nresx.Tools.Extensions;

namespace nresx.Tools.CodeParsers
{
    public interface ICodeParser
    {
        Dictionary<string, string> ParseLine( string line, string elementPath );
    }

    public class XamlCodeParser : QuottedCodeParser
    {
        private readonly Regex ResRegex = new( @"\s(Text|Header|Content|CommandText|OffContent|OnContent|PlaceholderText)\s*=\s*""(.*?)""",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
        private readonly Regex ElementNameRegex = new( @"\s(x:Name|Name)\s*=\s*""(.*?)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );
        private readonly Regex ElementTagRegex = new( @"<(.*?)\s+", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

        protected override MatchCollection MatchesTheLine( string line )
        {
            return ResRegex.Matches( line );
        }

        protected override bool GetValue( Match match, out string value )
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

        protected override string GenerateElementName( string line, string elementPath, Match match, ref int prevIndex )
        {
            string key = null;
            var prop = match.Groups[1].Value;
            var value = match.Groups[2].Value;
            var valueKey = string.Join( "", value.Split( ' ' ).Take( 2 ).Select( i => i.ToFirstCapital() ) );

            // try to get element name
            var nameMatch = ElementNameRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
            if ( nameMatch.Success )
            {
                key = $"{elementPath}_{nameMatch.Groups[2].Value.ToFirstCapital()}.{prop}";
                prevIndex = match.Length + match.Index + 1;
            }

            //         <TextBlock Grid.Row="0" x:Name="SampleTitle" Text="The title" TextWrapping="Wrap" Margin="0,10,0,0" FontSize="28"/>
            // try to get tag name
            if ( key == null )
            {
                var tagMatch = ElementTagRegex.Match( line, prevIndex, match.Length + match.Index - prevIndex + 1 );
                if ( tagMatch.Success )
                {
                    key = $"{elementPath}_{tagMatch.Groups[1].Value.ToFirstCapital()}_{valueKey}.{prop}";
                    //prevIndex = match.Length + match.Index + 1;
                }
            }

            // get name from value
            if ( key == null )
            {
                key = $"{elementPath}_{valueKey}";
            }

            return key;
        }
    }

    public class CsCodeParser : QuottedCodeParser
    {
        private Regex AsmRegex = new( @"\[assembly:", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
        private Regex CommentRegex = new( @"^\s*///", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );
        private Regex ThrowRegex = new( @"\s+throw new ", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );

        protected override bool ValidateLine( string line )
        {
            // [assembly: AssemblyTitle("appUwp")]

            if ( AsmRegex.IsMatch( line ) ||
                 CommentRegex.IsMatch( line ) ||
                 ThrowRegex.IsMatch( line ) )
            {
                return false;
            }

            return true;
        }

        protected override string GenerateElementName( string line, string elementPath, Match match, ref int prevIndex )
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
    }

    public class QuottedCodeParser : ICodeParser
    {
        #region Private fields

        private readonly Dictionary<string, int> ElementsCounts = new Dictionary<string, int>();
        private readonly Regex ElementRegex = new( @"""(.*?)""", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant );

        #endregion

        protected virtual MatchCollection MatchesTheLine( string line )
        {
            return ElementRegex.Matches( line );
        }
        protected virtual bool ValidateLine( string line )
        {
            return true;
        }
        protected virtual bool GetValue( Match match, out string value )
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
        protected virtual string GenerateElementName( string line, string elementPath, Match match, ref int prevIndex )
        {
            var value = match.Groups[1].Value;
            return $"{elementPath}_{string.Join( "", value.Split( ' ' ).Take( 2 ).Select( i => i.ToFirstCapital() ) )}";
        }

        public Dictionary<string, string> ParseLine( string line, string elementPath )
        {
            var result = new Dictionary<string, string>();

            if ( !ValidateLine( line ) )
                return result;

            var prevIndex = 0;
            var matches = MatchesTheLine( line );
            foreach ( Match match in matches )
            {
                if( !GetValue( match, out var value ) ) continue;
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
