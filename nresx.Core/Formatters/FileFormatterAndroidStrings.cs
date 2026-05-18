using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // Android resources XML — typically named strings.xml under res/values[-<locale>]/.
    //   <resources>
    //     <string name="key">value</string>
    //     <!-- comment for next string -->
    //     <string name="other">...</string>
    //   </resources>
    //
    // Scope (MVP): <string> elements only. <string-array> and <plurals> are
    // deferred — they don't fit single-value ResourceElement cleanly.
    internal class FileFormatterAndroidStrings : IFileFormatter
    {
        public bool LoadResourceFile(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            return LoadRawElements( stream, out elements, out headers, out comments );
        }

        public bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers,
            out List<Comment> comments )
        {
            headers = [];
            comments = [];
            elements = [];

            var doc = XDocument.Load( stream );
            var root = doc.Root;
            if ( root == null || root.Name.LocalName != "resources" )
                return false;

            var ns = root.GetDefaultNamespace();
            var strings = root.Elements( ns + "string" ).ToList();

            var list = new List<ResourceElement>( strings.Count );
            foreach ( var s in strings )
            {
                var key = s.Attribute( "name" )?.Value ?? string.Empty;
                var raw = s.Value ?? string.Empty;

                // immediately preceding XML comment (skipping whitespace) is the element's note
                var prev = s.PreviousNode;
                while ( prev is XText t && string.IsNullOrWhiteSpace( t.Value ) )
                    prev = prev.PreviousNode;
                var commentText = ( prev as XComment )?.Value?.Trim() ?? string.Empty;

                list.Add( new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = key,
                    Value = UnescapeAndroid( raw ).ReplaceNewLine(),
                    Comment = commentText.ReplaceNewLine(),
                } );
            }

            elements = list;
            return true;
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null )
        {
            var resources = new XElement( "resources" );

            foreach ( var el in elements )
            {
                if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                    resources.Add( new XComment( $" {el.Comment.ReplaceNewLine()} " ) );

                resources.Add( new XElement( "string",
                    new XAttribute( "name", el.Key ?? string.Empty ),
                    EscapeAndroid( ( el.Value ?? string.Empty ).ReplaceNewLine() ) ) );
            }

            var doc = new XDocument(
                new XDeclaration( "1.0", "utf-8", null ),
                resources );

            doc.Save( stream );
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;

        // Android escape sequences: \\ \' \" \n \t
        // XML attribute/text escapes (&amp; &lt; etc.) are handled by XDocument.
        private static string UnescapeAndroid( string text )
        {
            if ( string.IsNullOrEmpty( text ) ) return text;
            const string placeholder = "";
            return text
                .Replace( @"\\", placeholder )
                .Replace( @"\'", "'" )
                .Replace( "\\\"", "\"" )
                .Replace( @"\n", "\n" )
                .Replace( @"\t", "\t" )
                .Replace( placeholder, @"\" );
        }

        private static string EscapeAndroid( string text )
        {
            if ( string.IsNullOrEmpty( text ) ) return text;
            return text
                .Replace( @"\", @"\\" )
                .Replace( "'", @"\'" )
                .Replace( "\n", @"\n" )
                .Replace( "\r", string.Empty )
                .Replace( "\t", @"\t" );
        }
    }
}
