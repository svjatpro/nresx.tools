using System.Collections.Generic;
using System.IO;
using System.Linq;
using MiniExcelLibs;
using nresx.Core.Extensions;

namespace nresx.Core.Formatters
{
    // XLSX translator-exchange format (RSX-138). One sheet, three columns: Key | Value | Comment.
    // Column names are recognized case-insensitively. Header row is required - translator tools
    // always emit headers, and the binary format gives us no clean way to auto-detect their
    // absence. Use CSV/TSV (RSX-137) for headerless table exchange.
    //
    // Backed by MiniExcel (MIT, lightweight; chosen over ClosedXML / EPPlus per RSX-138).
    internal class FileFormatterXlsx : IFileFormatter
    {
        private const string SheetName = "Resources";

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

            // MiniExcel returns each row as a dynamic ExpandoObject keyed by header-row column name.
            // Cast through `object` to keep static typing (no Microsoft.CSharp dependency on netstandard2.0).
            var rows = MiniExcel.Query( stream, useHeaderRow: true ).Cast<object>().ToList();
            var result = new List<ResourceElement>( rows.Count );

            foreach ( var rowObj in rows )
            {
                if ( rowObj is not IDictionary<string, object?> dict ) continue;
                var key = ReadCell( dict, "Key" );
                var value = ReadCell( dict, "Value" );
                var comment = ReadCell( dict, "Comment" );

                // Skip fully blank rows - translator tools sometimes pad sheets with empties.
                if ( string.IsNullOrEmpty( key ) && string.IsNullOrEmpty( value ) && string.IsNullOrEmpty( comment ) )
                    continue;

                var element = new ResourceElement
                {
                    Type = ResourceElementType.String,
                    Key = key ?? string.Empty,
                    Value = ( value ?? string.Empty ).ReplaceNewLine(),
                };
                // Empty comment cell → no comment (don't insert a translator-comment placeholder).
                if ( !string.IsNullOrEmpty( comment ) )
                    element.Comment = comment.ReplaceNewLine();

                result.Add( element );
            }

            elements = result;
            return true;
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string> headers,
            List<Comment> comments,
            ResourceFileOption? options = null )
        {
            var rows = elements.Select( e => new
            {
                Key = e.Key ?? string.Empty,
                Value = ( e.Value ?? string.Empty ).ReplaceNewLine(),
                Comment = ( e.Comment ?? string.Empty ).ReplaceNewLine(),
            } ).ToList();

            MiniExcel.SaveAs( stream, rows, sheetName: SheetName );
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;

        // Case-insensitive column lookup. MiniExcel preserves casing from the header row,
        // so we look up by ignoring case rather than forcing a particular capitalization.
        private static string? ReadCell( IDictionary<string, object?> row, string columnName )
        {
            foreach ( var kv in row )
            {
                if ( string.Equals( kv.Key, columnName, System.StringComparison.OrdinalIgnoreCase ) )
                    return kv.Value?.ToString();
            }
            return null;
        }
    }
}
