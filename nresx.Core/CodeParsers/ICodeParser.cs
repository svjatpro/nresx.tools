using System;

namespace nresx.Tools.CodeParsers
{
    public interface ICodeParser
    {
        void ProcessNextLine( 
            string line, string elementPath,
            Func<string, string, bool> validateElement,
            Action<string, string> extractResourceElement,
            Action<string> writeProcessedLine );
    }
}