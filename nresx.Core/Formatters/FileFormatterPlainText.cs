using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace nresx.Tools.Formatters
{
    /// <summary>
    /// Plain test resource file in simple "key: value" format
    /// </summary>
    internal class FileFormatterPlainText : IFileFormatter
    {
        public bool LoadResourceFile( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            if ( LoadRawElements( stream, out var raw ) )
            {
                elements = raw;
                return true;
            }

            elements = null;
            return false;
        }

        public bool LoadRawElements( Stream stream, out IEnumerable<ResourceElement> elements )
        {
            using var reader = new StreamReader( stream );

            var result = new List<ResourceElement>();
            var elementLines = new List<string>();
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine();

                if ( string.IsNullOrWhiteSpace( line ) ) // empty line as a delimiter of elements
                {
                    result.Add( ParseElement( elementLines ) );
                    elementLines.Clear();
                }
                else
                {
                    elementLines.Add( line );
                }
            }

            if ( elementLines.Any() )
                result.Add( ParseElement( elementLines ) );

            elements = result;
            return true;
        }
        private ResourceElement ParseElement( List<string> elementLines )
        {
            var text = string.Join( Environment.NewLine, elementLines );
            return new()
            {
                Key = text, // ?generated key?
                Value = text
            };
        }

        public void SaveResourceFile( Stream stream, IEnumerable<ResourceElement> elements )
        {
            using var writer = new StreamWriter( stream );

            // write elements
            foreach ( var element in elements )
            {
                writer.WriteLine( $"{element.Value ?? string.Empty}" );
                writer.WriteLine();
            }
        }

        public bool ElementHasKey => false;
        public bool ElementHasComment => false;
    }
}