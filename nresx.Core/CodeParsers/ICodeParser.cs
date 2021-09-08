using System.Collections.Generic;

namespace nresx.Tools.CodeParsers
{
    public interface ICodeParser
    {
        string ExtractFromLine( string line, string elementPath, out Dictionary<string, string> elements );
    }
}