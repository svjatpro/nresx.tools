using System.Collections.Generic;

namespace nresx.Tools.CodeParsers
{
    public interface ICodeParser
    {
        Dictionary<string, string> ParseLine( string line, string elementPath );
    }
}