using System;
using System.Collections.Generic;

namespace nresx.Tools.CodeParsers
{
    public interface ICodeParser
    {
        void ProcessNextLine( 
            string line, string elementPath,
            Func<string, string, string> processExtractedElement,
            Action<string> writeProcessedLine );

        string IncrementKey( string key, List<ResourceElement> elements );
    }
}