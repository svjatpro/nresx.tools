using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // XLIFF 1.2 - OASIS standard, widely supported by TMS platforms (Crowdin, Lokalise, Phrase).
    // XLIFF 2.0 exists but TMS adoption lags; revisit if requested.
    //
    // Semantic note: XLIFF natively separates source and target. nresx's model is
    // "one file = one language", so on load we prefer <target> (falling back to
    // <source> if empty), and on save we write the value as both <source> and
    // <target>. That's a lossy roundtrip vs full XLIFF workflows, but it makes
    // "convert any format ↔ XLIFF" work for the common cases.
    internal class FileFormatterXliff : IFileFormatter
    {
        private static readonly XNamespace Ns = "urn:oasis:names:tc:xliff:document:1.2";

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
            if ( root == null || root.Name.LocalName != "xliff" )
                return false;

            var ns = root.GetDefaultNamespace();

            // walk all <file>/<body>/<trans-unit> across multiple <file> entries
            var transUnits = root.Descendants( ns + "trans-unit" ).ToList();

            elements = transUnits.Select( unit =>
            {
                var key = unit.Attribute( "id" )?.Value ?? string.Empty;
                var source = unit.Element( ns + "source" )?.Value;
                var target = unit.Element( ns + "target" )?.Value;
                var note = unit.Element( ns + "note" )?.Value;

                var raw = ( !string.IsNullOrEmpty( target ) ? target : source ) ?? string.Empty;
                return new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = key,
                    Value = raw.ReplaceNewLine(),
                    Comment = ( note ?? string.Empty ).ReplaceNewLine(),
                };
            } ).ToList();

            // pull language from the first <file> element if present
            var firstFile = root.Descendants( ns + "file" ).FirstOrDefault();
            if ( firstFile != null )
            {
                var targetLang = firstFile.Attribute( "target-language" )?.Value;
                if ( !string.IsNullOrWhiteSpace( targetLang ) )
                    headers[ResourceFileHeaders.Language] = targetLang!;
            }

            return true;
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null )
        {
            var targetLang = headers.TryGetValue( ResourceFileHeaders.Language, out var lang ) ? lang : string.Empty;
            const string sourceLang = "en";

            var body = new XElement( Ns + "body",
                elements.Select( el =>
                {
                    var value = ( el.Value ?? string.Empty ).ReplaceNewLine();
                    var unit = new XElement( Ns + "trans-unit",
                        new XAttribute( "id", el.Key ?? string.Empty ),
                        new XElement( Ns + "source", value ),
                        new XElement( Ns + "target", value ) );
                    if ( !string.IsNullOrWhiteSpace( el.Comment ) )
                        unit.Add( new XElement( Ns + "note", el.Comment.ReplaceNewLine() ) );
                    return unit;
                } ) );

            var file = new XElement( Ns + "file",
                new XAttribute( "source-language", sourceLang ),
                new XAttribute( "target-language", targetLang ),
                new XAttribute( "datatype", "plaintext" ),
                new XAttribute( "original", "messages" ),
                body );

            var doc = new XDocument(
                new XDeclaration( "1.0", "UTF-8", null ),
                new XElement( Ns + "xliff",
                    new XAttribute( "version", "1.2" ),
                    file ) );

            doc.Save( stream );
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;
    }
}
