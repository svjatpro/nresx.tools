using nresx.Tools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace nresx.Tools.Formatters
{
    internal class FileFormatterPo : IFileFormatter
    {
        #region Private fields

        private const string MsgIdTag = "msgid";
        private const string MsgStrTag = "msgstr";


        #endregion

        public bool LoadResourceFile( 
            Stream stream, 
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers )
        {
            if ( LoadRawElements( stream, out var raw, out headers ) )
            {
                var dictionary = new Dictionary<string, ResourceElement>();
                var elementsList = raw.ToList();
                foreach (var el in elementsList)
                {
                    if (string.IsNullOrWhiteSpace(el.Key) || dictionary.ContainsKey(el.Key)) 
                        continue;
                    dictionary.Add(el.Key, el);
                }

                elements = dictionary.Values.ToList();
                return true;
            }

            elements = [];
            return false;
        }

        public bool LoadRawElements(
            Stream stream,
            out IEnumerable<ResourceElement> elements,
            out Dictionary<string, string> headers )
        {
            using var reader = new StreamReader( stream );
            headers = [];

            var result = new List<ResourceElement>();
            var elementLines = new List<string>();
            while ( !reader.EndOfStream )
            {
                var line = reader.ReadLine();

                if ( string.IsNullOrWhiteSpace( line ) )
                {
                    var el = ParseElement( elementLines );
                    if ( string.IsNullOrWhiteSpace( el.Key ) && !result.Any() )
                        headers = ParseHeaders( el.Value );
                    else
                        result.Add( el );
                    elementLines.Clear();
                }
                else
                {
                    elementLines.Add( line );
                }
            }
            if( elementLines.Any() )
                result.Add( ParseElement( elementLines ) );

            elements = result;
            
            return true;
        }

        private Dictionary<string, string> ParseHeaders( string lines )
        {
            return lines
                .Split( [Environment.NewLine], StringSplitOptions.RemoveEmptyEntries )
                .Select( h => h.Split( [": "], StringSplitOptions.RemoveEmptyEntries ) )
                .Where( parts => parts.Length == 2 )
                .Select( parts => new { key = parts[0], value = parts[1] } )
                .ToDictionary( h => h.key, h => h.value );
        }

        private enum ElementParseState { None, Comment, MsgId, MsgStr }
        private ResourceElement ParseElement( List<string> lines )
        {
            var element = new ResourceElement{ Type = ResourceElementType.String };
            var state = ElementParseState.None;
            var propLines = new List<string>();

            foreach ( var line in lines )
            {
                switch (line)
                {
                    case "#":
                        break;
                    case var _ when line.StartsWith( "# " ):
                        ParseProperty( ElementParseState.Comment );

                        var comment = line.Length > 2 ? line.Substring( 2 ) : string.Empty;
                        element.Comment = comment;
                        break;
                    case var _ when line.StartsWith( $"{MsgIdTag} " ):
                        ParseProperty( ElementParseState.MsgId );

                        var r1 = new Regex( @"msgid\s+""(.*)""" ).Match( line );
                        if ( r1.Success && r1.Groups.Count > 1 )
                        {
                            var value = r1.Groups[1].Value;
                            propLines.Add( value );
                        }
                        break;
                    case var _ when line.StartsWith( $"{MsgStrTag} " ):
                        ParseProperty( ElementParseState.MsgStr );

                        var r2 = new Regex( @"msgstr\s+""(.*)""" ).Match( line );
                        if ( r2.Success && r2.Groups.Count > 1 )
                        {
                            var value = r2.Groups[1].Value;
                            propLines.Add( value );
                        }
                        break;
                    case var _ when line.Trim( ' ' ).StartsWith( "\"" ):
                        if ( state != ElementParseState.None )
                        {
                            var r3 = new Regex( @"""(.*)""" ).Match( line );
                            if ( r3.Success && r3.Groups.Count > 1 )
                            {
                                propLines.Add( r3.Groups[1].Value );
                            }
                        }
                        break;
                }
            }
            ParseProperty( ElementParseState.None );

            return element;

            void ParseProperty( ElementParseState nextProp )
            {
                if ( propLines.Any() )
                {
                    var value = string.Join( "", propLines ).Replace( @"\n", Environment.NewLine );
                    switch ( state )
                    {
                        case ElementParseState.MsgId:
                            element.Key = value;
                            break;
                        case ElementParseState.MsgStr:
                            element.Value = value;
                            break;
                    }
                    propLines.Clear();
                }

                state = nextProp;
            }
        }

        public void SaveResourceFile(
            Stream stream,
            IEnumerable<ResourceElement> elements,
            Dictionary<string, string>? headers,
            ResourceFileOption? options = null )
        {
            using var writer = new StreamWriter( stream );

            // write headers
            if (headers != null)
            {
                writer.WriteLine("#");
                WriteMultilineProperty(string.Empty, MsgIdTag);
                
                var headersPrepared = string.Join(
                    Environment.NewLine,
                    [/*string.Empty,*/..headers.Select(h => $"{h.Key}: {h.Value}\\n")]);
                WriteMultilineProperty( headersPrepared, MsgStrTag );

                writer.WriteLine();
            }

            // write elements
            foreach ( var element in elements )
            {
                // write comment
                writer.WriteLine( $"# {element.Comment ?? string.Empty}" );

                // write key
                WriteMultilineProperty( element.Key, MsgIdTag );

                // write value
                WriteMultilineProperty( element.Value, MsgStrTag );

                // write empty line between elements
                writer.WriteLine();
            }

            return;

            void WriteMultilineProperty( string property, string tag )
            {
                //var propLines = property.SplitLines().Select( l => $"{l}\\n" ).ToList();
                var propLines = property.SplitLines().ToList(); //.Select( l => $"{l}\\n" ).ToList();

                if ( propLines.Count > 1 )
                {
                    for (var i = 0; i < propLines.Count - 1; i++)
                    {
                        if( !string.IsNullOrWhiteSpace( propLines[i] ) )
                            propLines[i] = $"{propLines[i]}\\n";
                    }
                    propLines.Insert( 0, string.Empty );
                }

                if ( propLines.Any() )
                {
                    writer.WriteLine( $"{tag} \"{propLines[0]}\"" );
                }

                if ( propLines.Count > 1 )
                {
                    foreach ( var valueLine in propLines.Skip( 1 ) )
                    {
                        writer.WriteLine( $"\"{valueLine}\"" );
                    }
                }
            }
        }

        public bool ElementHasKey => true;
        public bool ElementHasComment => true;
    }
}
