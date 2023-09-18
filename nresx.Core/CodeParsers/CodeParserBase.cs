using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace nresx.Tools.CodeParsers
{
    public abstract class CodeParserBase : ICodeParser
    {
        #region Private members

        protected readonly Regex KeyIndexRegex = new( @"(\d+)$",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

        protected string IncrementKeyName( string key, List<ResourceElement> elements )
        {
            var keyIndexMatch = KeyIndexRegex.Match( key );
            var index = 0;
            var keyCore = key;
            string newKey;
            if ( keyIndexMatch.Success )
            {
                index = int.Parse( keyIndexMatch.Groups[1].Value );
                keyCore = key.Substring( 0, key.Length - keyIndexMatch.Groups[1].Value.Length );
            }

            do { newKey = $"{keyCore}{++index}"; }
            while ( elements.Any( el => el.Key == newKey ) );

            return newKey;
        }

        #endregion

        public abstract void ProcessNextLine( string line, string elementPath,
            Func<string, string, string> processExtractedElement, 
            Action<string> writeProcessedLine );

        public virtual string IncrementKey( string key, List<ResourceElement> elements )
        {
            return IncrementKeyName( key, elements );
        }
    }
}