using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace NResx.Tools.Formatters
{
    internal class FileFormatterYaml : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out List<ResourceElement> elements )
        {
            // todo: replace stub implementation
            var resRegex = new Regex( @"^\s*([a-zA-Z0-9_.]+)\s*:\s*""([^""]*)""\s*$", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant );

            elements = new List<ResourceElement>();
            using var reader = new StreamReader( stream );
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine();
                if ( string.IsNullOrWhiteSpace( line ) )
                    continue;
                var match = resRegex.Match( line );
                if ( match.Success && match.Groups.Count > 1 )
                {
                    elements.Add( new ResourceElement
                    {
                        Key = match.Groups[1].Value,
                        Value = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty
                    } );
                }
            }

            return true;
        }

        public void SaveResourceFile( Stream stream, List<ResourceElement> elements )
        {
            using var writer = new StreamWriter( stream );
            foreach ( var element in elements )
            {
                writer.WriteLine( $"{element.Key}: \"{element.Value}\"" );
            }
        }
    }
}